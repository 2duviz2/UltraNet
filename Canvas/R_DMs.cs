namespace UltraNet.Canvas;

using Jaket.Sam;
using Steamworks;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

public class R_DMs : MonoBehaviour
{
    public TMP_InputField Field;
    public TMP_InputField ID;
    public TMP_InputField Title;
    public TMP_InputField Content;
    public Image pfp;
    public GameObject Online;

    public string chat = "PENIS";
    public string dmName = "PENIS";
    public string id = "PENIS";
    public string id2 = "PENIS";

    public void Start()
    {
        Field.onSubmit.AddListener((t) => { SendMessage(t); });
        Field.ActivateInputField();

        Title.text = dmName;
        ID.text = $"#{id2}";

        ContentManager.CurrentChatField = Field;

        R_Friends.LoadPfp(id, pfp);
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

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileUrl, new() { { "token", ContentManager.GetToken() }, { "steamid", id } }, (json) =>
        {
            var profiles = ProfileParser.Parse(json);

            if (profiles.Count >= 1)
            {
                var p = profiles[0];

                if (p.status.ToLower() == "online")
                {
                    Online.gameObject.SetActive(true);
                }
                else
                {
                    Online.gameObject.SetActive(false);
                }
            }
        }));

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.chatUrl, new() { { "token", ContentManager.GetToken() }, { "chat", chat } }, (json) =>
        {
            checking = false;

            var messages = ChatParser.Parse(json);
            var lastid = "";
            var newText = "";
            var lastMsg = "";

            foreach (var message in messages)
            {
                newText += $"\n";
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

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.sendChatUrl, new() { { "token", ContentManager.GetToken() }, { "content", t }, { "chat", chat } }, (json) =>
        {
            checking = false;
            CheckForMessages();
        }));
    }

    public void OpenProfile()
    {
        var window = ContentManager.instance.SpawnWindowCustomID("profile", $"profile.{id}");
        var profile = window.GetComponent<R_Profile>();
        profile.LoadProfile(id);
    }
}
