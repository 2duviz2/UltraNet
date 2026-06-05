namespace UltraNet.Canvas;

using Jaket.Sam;
using System;
using System.Collections.Generic;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using static UnityEngine.InputSystem.InputRemoting;

public class R_Chat : MonoBehaviour
{
    public TMP_InputField Field;

    public TMP_InputField Content;

    public void Start()
    {
        Field.onSubmit.AddListener((t) => { SendMessage(t); });
        Field.ActivateInputField();

        ContentManager.CurrentChatField = Field;
    }

    public void Update()
    {
        if (!checking)
        {
            checkTimer += Time.unscaledDeltaTime;

            if (checkTimer >= checkCooldown)
            {
                CheckForMessages();
            }
        }
    }

    bool checking = false;
    float checkCooldown = .2f;
    float checkTimer = 0;

    public void CheckForMessages()
    {
        if (checking) return;
        checkTimer = 0;
        checking = true;

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.chatUrl, new() { { "token", ContentManager.GetToken() } }, (json) =>
        {
            checking = false;

            var messages = ChatParser.Parse(json);
            var lastid = "";
            var newText = "";
            var lastMsg = "";

            foreach (var message in messages)
            {
                newText += $"\n";
                if (message.authorid != lastid)
                    newText += $"<color=#00000011>#{message.authorid}</color> ";
                newText += $"({message.name}): {TextParser.Parse(message.content)}";
                lastMsg = TextParser.Parse(message.content);

                lastid = message.authorid;
            }

            if (Content.text != newText)
            {
                Content.text = newText;
                SamAPI.TryPlay(TTSParser.Parse(lastMsg), ContentManager.instance.source);
            }
        }));
    }

    public void SendMessage(string t)
    {
        if (t == "") return;
        Field.text = "";
        Field.ActivateInputField();

        checking = true;

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.sendChatUrl, new() { { "token", ContentManager.GetToken() }, { "content", t } }, (json) =>
        {
            checking = false;
            CheckForMessages();
        }));
    }
}

[Serializable]
public class ChatMessage
{
    public string author;
    public string authorid;
    public string chat;
    public string content;
    public string custom;
    public string time;
    public string name;
}

[Serializable]
public class ChatMessageList
{
    public List<ChatMessage> messages;
}

public static class ChatParser
{
    public static List<ChatMessage> Parse(string json)
    {
        try
        {
            string wrapped = "{ \"messages\": " + json + "}";
            ChatMessageList result = JsonUtility.FromJson<ChatMessageList>(wrapped);
            return result.messages;
        }
        catch (Exception)
        {
            return [];
        }
    }
}