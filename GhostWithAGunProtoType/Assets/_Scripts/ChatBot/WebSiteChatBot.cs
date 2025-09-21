using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebSiteChatBot : MonoBehaviour
{
    [SerializeField] GameObject _aiMessage, _playerMessage;
    [SerializeField] GameObject _textField;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] ChatDatabase _chatDatabase;
    [SerializeField] ScrollRect _scrollRect;

    private ChatBotRuleMatcher _matcher;
    private WordParser _wordParser;


    //TODO move this to a scriptable object for easier designer editing
    Dictionary<string, string> synonyms = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["rooms"] = "room",
        ["cabins"] = "cabin",
        ["booked"] = "book",
        ["booking"] = "book",
        ["reservation"] = "book",
        ["check-in"] = "check in",
        ["checkin"] = "check in",
        ["check-out"] = "check out",
        ["checkout"] = "check out",
        ["haunting"] = "haunt",
        ["ghosts"] = "ghost",
    };

    private void Start()
    {
        _wordParser = new WordParser(
            stopwords: null,         // use defaults
            synonyms: synonyms,
            maxNGram: 3,
            enableStemming: true
        );

        _matcher = new ChatBotRuleMatcher(_chatDatabase);
    }

    public void PlayerSendMessage(string message)
    {
        _inputField.text = "";

        DisplayMessage(message, true);
        ParseResult result = _wordParser.Parse(message);
        ChatRule bestRule = _matcher.FindBestMatch(result);

        string reply = _matcher.GetResponse(bestRule);
        DisplayMessage(reply, false);
    }


    private void DisplayMessage(string text, bool isPlayer)
    {
        GameObject newMessage = Instantiate(isPlayer ? _playerMessage : _aiMessage, _textField.transform);
        newMessage.GetComponentInChildren<TextMeshProUGUI>().text = text;
        // force rebuild of layout so ScrollRect knows the new size
        Canvas.ForceUpdateCanvases();

        // snap to bottom
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}
