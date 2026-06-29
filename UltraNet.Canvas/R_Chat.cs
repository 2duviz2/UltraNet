using System.Collections.Generic;
using Jaket.Sam;
using TMPro;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Canvas;

public class R_Chat : MonoBehaviour
{
	public TMP_InputField Field;

	public TMP_InputField Content;

	private bool checking = false;

	private float checkCooldown = 0.2f;

	private float checkTimer = 0f;

	public void Start()
	{
		Field.onSubmit.AddListener(delegate(string t)
		{
			SendMessage(t);
		});
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

	public void CheckForMessages()
	{
		if (checking)
		{
			return;
		}
		checkTimer = 0f;
		checking = true;
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/chat", new Dictionary<string, string> { 
		{
			"token",
			ContentManager.GetToken()
		} }, delegate(string json)
		{
			checking = false;
			List<ChatMessage> list = ChatParser.Parse(json);
			string text = "";
			string text2 = "";
			string text3 = "";
			foreach (ChatMessage current in list)
			{
				text2 += "\n";
				string text4 = "";
				string text5 = "";
				if (current.author == "SYSTEM")
				{
					text4 = "<color=#228822>";
					text5 = "</color>";
				}
				if (current.name != "" && current.author != "SYSTEM")
				{
					text2 = text2 + "<color=#00000077>(</color><color=#00000077><link=" + current.author + ">" + current.name + "</link></color><color=#00000077>)</color> ";
				}
				text2 = text2 + text4 + TextParser.Parse(current.content) + text5;
				text3 = TextParser.Parse(current.content);
				text = current.authorid;
			}
			if (Content.text != text2)
			{
				Content.text = text2;
				SamAPI.TryPlay(TTSParser.Parse(text3), ContentManager.instance.source);
			}
		}));
	}

	public new void SendMessage(string t)
	{
		if (!(t == ""))
		{
			Field.text = "";
			Field.ActivateInputField();
			checking = true;
			Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/chat/sendMessageV2", new Dictionary<string, string>
			{
				{
					"token",
					ContentManager.GetToken()
				},
				{ "content", t }
			}, delegate
			{
				checking = false;
				CheckForMessages();
			}));
		}
	}
}
