using System.Collections.Generic;
using Jaket.Sam;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

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
		Title.text = dmName;
		ID.text = "#" + id2;
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

	public void CheckForMessages()
	{
		if (checking)
		{
			return;
		}
		checkTimer = 0f;
		checking = true;
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/profileV2", new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "steamid", id }
		}, delegate(string json)
		{
			List<Profile> list = ProfileParser.Parse(json);
			if (list.Count >= 1)
			{
				Profile profile = list[0];
				if (profile.status.ToLower().Contains("online"))
				{
					Online.gameObject.SetActive(value: true);
					Title.text = profile.name + "\n<color=#333333>" + profile.level + "</color>";
				}
				else
				{
					Online.gameObject.SetActive(value: false);
				}
			}
		}));
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/chat", new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "chat", chat }
		}, delegate(string json)
		{
			checking = false;
			List<ChatMessage> list = ChatParser.Parse(json);
			string text = "";
			string text2 = "";
			string text3 = "";
			foreach (ChatMessage current in list)
			{
				string[] array = TextParser.Parse(current.content).Split(' ');
				for (int i = 0; i < array.Length; i++)
				{
					string text4 = array[i];
					if (text4.StartsWith("https://") || text4.StartsWith("http://"))
					{
						array[i] = "<color=#4AF><link=" + text4 + ">" + text4 + "</link></color>";
					}
				}
				string text5 = string.Join(' ', array);
				text2 += "\n";
				text2 = text2 + "(" + current.name + "): " + text5;
				text3 = text5;
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
				{ "content", t },
				{ "chat", chat }
			}, delegate
			{
				checking = false;
				CheckForMessages();
			}));
		}
	}

	public void OpenProfile()
	{
		GameObject window = ContentManager.instance.SpawnWindowCustomID("profile", "profile." + id);
		R_Profile profile = window.GetComponent<R_Profile>();
		profile.LoadProfile(id);
	}

	public static void OpenProfileStatic(string id)
	{
		GameObject window = ContentManager.instance.SpawnWindowCustomID("profile", "profile." + id);
		R_Profile profile = window.GetComponent<R_Profile>();
		profile.LoadProfile(id);
	}
}
