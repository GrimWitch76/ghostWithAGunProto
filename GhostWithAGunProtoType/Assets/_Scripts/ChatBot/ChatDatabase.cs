using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chat Database", menuName = "Chatbot/Chat Database")]
public class ChatDatabase : ScriptableObject
{
    public List<ChatRule> Rules = new List<ChatRule>();

    public ChatRule FindById(string id)
    {
        return Rules.Find(r => r.Id == id);
    }
}