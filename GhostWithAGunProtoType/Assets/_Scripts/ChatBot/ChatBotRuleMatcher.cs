using UnityEngine;
using System.Linq;

public class ChatBotRuleMatcher
{
    private ChatDatabase _dataBase;
    private readonly System.Random _random = new System.Random();

    public ChatBotRuleMatcher(ChatDatabase dataBase)
    {
        _dataBase = dataBase;
    }

    public ChatRule FindBestMatch(ParseResult parse)
    {
        ChatRule best = null;
        foreach (var rule in _dataBase.Rules)
        {
            if (rule.Keywords == null || rule.Keywords.Count == 0) continue; // skip fallback for now

            if (rule.Keywords.Any(k => parse.AllTerms.Contains(k))) 
            {
                if (best == null || rule.Priority > best.Priority)
                    best = rule;
            }
        }

        // If nothing matched, fallback = rule with empty keywords
        return best != null ? best : _dataBase.Rules.FirstOrDefault(r => r.Keywords.Count == 0);
    }

    public string GetResponse(ChatRule rule)
    {
        if (rule == null || rule.Responses.Count == 0)
        {
            Debug.LogError("Chatbot error no response found");
            return "If you're seeing this one of the devs messed up"; // final fallback to prevent bot from breaking if it finds no possible responses
        }
        int idx = _random.Next(rule.Responses.Count);
        return rule.Responses[idx];
    }
}
