using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class ParseResult
{
    public string Original { get; }
    public List<string> Tokens { get; }   // canonical single-word terms
    public List<string> NGrams { get; }   // canonical multi-word phrases (bigrams/trigrams)
    public List<string> AllTerms { get; } // NGrams first, then Tokens (deduped)

    public ParseResult(string original, List<string> tokens, List<string> ngrams, List<string> allTerms)
    {
        Original = original;
        Tokens = tokens;
        NGrams = ngrams;
        AllTerms = allTerms;
    }
}

public class WordParser
{
    private static Regex SplitRegex = new Regex(@"[^a-z0-9']+", RegexOptions.Compiled); 
    private HashSet<string> _stopwords;
    private Dictionary<string, string> _synonyms;
    private int _maxNGram;
    private bool _enableStemming;

    public WordParser(
        IEnumerable<string> stopwords = null,
        IDictionary<string, string> synonyms = null,
        int maxNGram = 3,                    // up to trigrams for phrases like "late check out"
        bool enableStemming = true)          // tiny heuristic stemmer
    {
        _stopwords = new HashSet<string>(stopwords ?? DefaultStopwords(), StringComparer.Ordinal);
        _synonyms = new Dictionary<string, string>(synonyms ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        _maxNGram = Math.Max(1, Math.Min(5, maxNGram));
        _enableStemming = enableStemming;
    }

    public ParseResult Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParseResult(input ?? string.Empty, new List<string>(), new List<string>(), new List<string>());

        // 1) Normalize
        string normalized = Normalize(input);

        // 2) Tokenize 
        var rawTokens = SplitRegex.Split(normalized)
                                  .Where(t => t.Length > 0)
                                  .ToList();

        // 3) Filter
        var filtered = rawTokens.Where(t => t.Length >= 2 && !_stopwords.Contains(t)).ToList();

        // 4) Canonicalize
        var canon = filtered.Select(Canonicalize).Where(t => t.Length > 0).ToList();

        // 5) N-grams 
        var ngrams = BuildNGrams(canon, _maxNGram);

        // 6) Build AllTerms:
        var all = new List<string>();
        foreach (var n in ngrams) if (!all.Contains(n)) all.Add(n);
        foreach (var t in canon) if (!all.Contains(t)) all.Add(t);

        return new ParseResult(input, canon, ngrams, all);
    }

    private string Normalize(string s)
    {
        // Lowercase + trim
        s = s.ToLowerInvariant().Trim();

        // Replace common separators with spaces
        s = s.Replace('_', ' ').Replace('-', ' ');

        // Unicode
        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        s = sb.ToString().Normalize(NormalizationForm.FormC);

        s = s.Replace('’', '\'').Replace('‘', '\'').Replace('“', '"').Replace('”', '"');

        return s;
    }

    private string Canonicalize(string token)
    {
        // Synonym map first (e.g., "rooms" -> "room")
        if (_synonyms.TryGetValue(token, out var mapped))
            token = mapped;

        if (_enableStemming)
            token = TinyStem(token);

        // Apply synonym again after stemming (e.g., "booked" -> "book")
        if (_synonyms.TryGetValue(token, out var mapped2))
            token = mapped2;

        return token;
    }
    
    private static List<string> BuildNGrams(List<string> tokens, int maxN)
    {
        var list = new List<string>();
        int nMax = Math.Min(maxN, Math.Max(1, tokens.Count));
        for (int n = nMax; n >= 2; n--) // phrases first (e.g., trigrams before bigrams)
        {
            for (int i = 0; i <= tokens.Count - n; i++)
            {
                list.Add(string.Join(" ", tokens.Skip(i).Take(n)));
            }
        }
        return list;
    }

    //removes common word endings eg booking vs book
    private string TinyStem(string t)
    {
        if (t.Length > 5 && t.EndsWith("ing")) return t.Substring(0, t.Length - 3);
        if (t.Length > 4 && (t.EndsWith("ed") || t.EndsWith("es"))) return t.Substring(0, t.Length - 2);
        if (t.Length > 3 && t.EndsWith("s") && !t.EndsWith("ss")) return t.Substring(0, t.Length - 1);
        return t;
    }

    //These are just common filler words that don't need to be parsed.
    //TODO Let a designer add / remove these with a scriptable object
    private static IEnumerable<string> DefaultStopwords() => new[]
{
        "the","a","an","is","are","am","i","you","we","he","she","it","they",
        "to","of","in","on","at","for","and","or","not","do","does","did",
        "with","please","can","could","would","should","may","might","be","been",
        "from","this","that","these","those","my","your","our","their"
    };


}
