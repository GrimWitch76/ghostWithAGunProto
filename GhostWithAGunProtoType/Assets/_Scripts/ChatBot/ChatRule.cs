using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chat Rule", menuName = "Chatbot/Chat Rule")]
public class ChatRule : ScriptableObject
{
    [Header("Unique ID for this rule (used for chaining)")]
    public string Id;

    [Header("Trigger keywords (lowercase, no punctuation)")]
    public List<string> Keywords = new List<string>();

    [Header("Priority (higher beats lower if multiple match)")]
    public int Priority = 1;

    [Header("Possible responses (randomly chosen)")]
    [TextArea(2, 4)]
    public List<string> Responses = new List<string>();

    [Header("Optional next rule to auto-trigger")]
    public string NextRuleId;
}