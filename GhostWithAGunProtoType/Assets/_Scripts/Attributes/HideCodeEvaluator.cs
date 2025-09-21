using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public static class HideCodeEvaluator
{
    private static readonly Dictionary<string, Type> _enumTypeCache = new();
    private static bool _debug = false;

    public static void SetDebug(bool enabled) => _debug = enabled;

    public static bool ShouldHide(string code, object context)
    {
        if (_debug) Debug.Log($"Evaluating code: {code} on object: {context}");
        code = code.Replace(" ", "").Replace("\n", "").Replace("\r", "");

        if (code.StartsWith("IF(") && code.Contains(")HIDE();"))
        {
            int start = code.IndexOf("IF(") + 3;
            int end = code.IndexOf(")", start);
            if (end == -1)
            {
                if (_debug) Debug.LogWarning("Could not find closing paren for IF()");
                return false;
            }

            string condition = code.Substring(start, end - start);
            if (_debug) Debug.Log($"Extracted condition: {condition}");
            return EvaluateCondition(condition, context);
        }

        if (code == "HIDE();" || code.Trim() == "HIDE()")
        {
            if (_debug) Debug.Log("Matched unconditional HIDE()");
            return true;
        }

        if (_debug) Debug.Log("No pattern matched; default to visible.");
        return false;
    }

    private static bool EvaluateCondition(string condition, object context)
    {
        try
        {
            var tokens = Tokenize(condition);
            if (_debug) Debug.Log("Tokens: " + string.Join(", ", tokens));
            return EvaluateTokens(tokens, context);
        }
        catch (Exception ex)
        {
            if (_debug) Debug.LogWarning("Evaluation failed: " + ex);
            return false;
        }
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        int length = input.Length;
        for (int i = 0; i < length;)
        {
            char c = input[i];

            // skip whitespace
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // two-char ops: &&, ||, !=, ==
            if (i + 1 < length)
            {
                if (c == '&' && input[i + 1] == '&')
                {
                    tokens.Add("&&");
                    i += 2;
                    continue;
                }
                if (c == '|' && input[i + 1] == '|')
                {
                    tokens.Add("||");
                    i += 2;
                    continue;
                }
                if (c == '!' && input[i + 1] == '=')
                {
                    tokens.Add("!=");
                    i += 2;
                    continue;
                }
                if (c == '=' && input[i + 1] == '=')
                {
                    tokens.Add("==");
                    i += 2;
                    continue;
                }
            }

            // single-char tokens
            if (c == '(' || c == ')' || c == '>' || c == '<' || c == '!')
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            // identifiers / numbers: [\w.]+
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                int start = i;
                i++;
                while (i < length)
                {
                    char nc = input[i];
                    if (char.IsLetterOrDigit(nc) || nc == '_' || nc == '.')
                    {
                        i++;
                        continue;
                    }
                    break;
                }
                tokens.Add(input.Substring(start, i - start));
                continue;
            }

            // anything else as single char
            tokens.Add(c.ToString());
            i++;
        }

        return tokens;
    }

    private static bool EvaluateTokens(List<string> tokens, object context)
    {
        bool? result = null;
        string op = null;

        for (int i = 0; i < tokens.Count; i++)
        {
            string t = tokens[i];

            if (t == "&&" || t == "||")
            {
                op = t;
                continue;
            }

            // unary negation
            if (t == "!")
            {
                if (i + 1 >= tokens.Count) break;
                bool val = GetBoolValue(tokens[++i], context);
                bool neg = !val;
                if (_debug) Debug.Log($"Negation: !{tokens[i]} => {neg}");
                result = result == null ? neg : ApplyOp(result.Value, neg, op);
                continue;
            }

            // comparisons
            if (i + 2 < tokens.Count && IsComparison(tokens[i + 1]))
            {
                string left = t;
                string cmp = tokens[++i];
                string right = tokens[++i];
                if (_debug) Debug.Log($"Compare: {left} {cmp} {right}");
                bool sub = Compare(left, cmp, right, context);
                result = result == null ? sub : ApplyOp(result.Value, sub, op);
                continue;
            }

            // standalone boolean or literal
            bool bval = GetBoolValue(t, context);
            if (_debug) Debug.Log($"Boolean token '{t}' => {bval}");
            result = result == null ? bval : ApplyOp(result.Value, bval, op);
        }

        if (_debug) Debug.Log($"Final result: {result}");
        return result ?? false;
    }

    private static bool ApplyOp(bool left, bool right, string op) =>
        op == "&&" ? left && right :
        op == "||" ? left || right :
        right;

    private static bool IsComparison(string token) =>
        token == ">" || token == "<" || token == "==" || token == "!=";

    private static bool Compare(string lhs, string cmp, string rhs, object context)
    {
        var a = GetValue(lhs, context);
        var b = GetValue(rhs, context);

        if (_debug) Debug.Log($"Comparing values: {lhs}={a}, {rhs}={b}");
        if (a == null || b == null) return false;

        // numeric compare
        if (TryFloat(a, out float fa) && TryFloat(b, out float fb))
        {
            return cmp switch
            {
                ">" => fa > fb,
                "<" => fa < fb,
                "==" => fa == fb,
                "!=" => fa != fb,
                _ => false
            };
        }

        // fallback to string/enum compare
        return cmp switch
        {
            "==" => a.ToString() == b.ToString(),
            "!=" => a.ToString() != b.ToString(),
            _ => false
        };
    }

    private static bool TryFloat(object v, out float f)
    {
        if (v is IConvertible)
        {
            try { f = Convert.ToSingle(v); return true; }
            catch { }
        }
        f = 0;
        return false;
    }

    private static bool GetBoolValue(string token, object context)
    {
        // handle literals
        if (token.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (token.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;

        var val = GetValue(token, context);
        if (val is bool b) return b;
        if (val != null)
        {
            try { return Convert.ToBoolean(val); }
            catch { if (_debug) Debug.LogWarning($"Cannot convert '{val}' to bool"); }
        }
        return false;
    }

    private static object GetValue(string token, object context)
    {
        if (_debug) Debug.Log($"GetValue('{token}') on {context.GetType().Name}");

        // numeric literal
        if (float.TryParse(token, out float num))
        {
            if (_debug) Debug.Log($"Parsed number: {num}");
            return num;
        }

        // instance field
        var type = context.GetType();
        var field = type.GetField(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null) return field.GetValue(context);

        // instance property
        var prop = type.GetProperty(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null) return prop.GetValue(context);

        // enum literal X.Y
        if (token.Contains("."))
        {
            var parts = token.Split('.');
            if (parts.Length == 2)
            {
                var en = parts[0];
                var ev = parts[1];
                if (!_enumTypeCache.TryGetValue(en, out var et))
                {
                    et = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.IsEnum && (t.Name == en || t.FullName.EndsWith("." + en)));
                    if (et != null) _enumTypeCache[en] = et;
                }
                if (et != null)
                {
                    try { return Enum.Parse(et, ev); }
                    catch { }
                }
            }
        }

        if (_debug) Debug.LogWarning($"Token '{token}' not found on context.");
        return null;
    }
}
