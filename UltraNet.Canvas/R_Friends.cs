using System.Collections.Generic;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

public class R_Friends : MonoBehaviour
{
	public Transform Container;

	public GameObject FriendItem;

	public GameObject RequestsHolder;

	private bool loading = false;

	private float timer = 0f;

	private string lastFriends = "";

	public void Start()
	{
		Fetch();
	}

	public void Update()
	{
		bool requests = false;
		foreach (Notification noti in NotificationListener.notifications)
		{
			if (noti.type == "friendRequest")
			{
				requests = true;
			}
		}
		RequestsHolder.SetActive(requests);
		if (!loading)
		{
			timer += Time.unscaledDeltaTime;
			if (timer >= 3f)
			{
				timer = 0f;
				Fetch();
			}
		}
	}

	public void Fetch()
	{
		if (loading)
		{
			return;
		}
		loading = true;
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/getFriends", new Dictionary<string, string> { 
		{
			"token",
			ContentManager.GetToken()
		} }, delegate(string json)
		{
			loading = false;
			List<Profile> list = ProfileParser.Parse(json);
			string text = "";
			foreach (Profile current in list)
			{
				text = text + current.id + "," + current.name + "," + current.level + "," + current.status + ",";
			}
			foreach (Notification current2 in NotificationListener.notifications)
			{
				text = text + current2.content + current2.read + current2.type;
			}
			if (!(text != lastFriends))
			{
				return;
			}
			lastFriends = text;
			CleanUI();
			foreach (Profile profile in list)
			{
				GameObject gameObject = Object.Instantiate(FriendItem, Container);
				LoadPfp(profile.id, gameObject.transform.GetChild(0).GetChild(0).GetComponent<Image>());
				gameObject.transform.GetChild(1).GetComponent<TMP_InputField>().text = profile.name;
				GameObject gameObject2 = gameObject.transform.GetChild(2).gameObject;
				GameObject gameObject3 = gameObject.transform.GetChild(3).gameObject;
				GameObject gameObject4 = gameObject.transform.GetChild(4).gameObject;
				gameObject4.SetActive(profile.status.ToLower().Contains("online"));
				TMP_InputField component = gameObject.transform.GetChild(5).gameObject.GetComponent<TMP_InputField>();
				component.text = profile.level;
				bool active = false;
				foreach (Notification current3 in NotificationListener.notifications)
				{
					if (current3.type == "dm" && current3.content == profile.id)
					{
						active = true;
					}
				}
				gameObject3.SetActive(active);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					GameObject gameObject5 = ContentManager.instance.SpawnWindowCustomID("friendChat", "friendChat." + profile.dms);
					if ((bool)gameObject5)
					{
						R_DMs component2 = gameObject5.GetComponent<R_DMs>();
						component2.id = profile.id;
						component2.id2 = profile.id2;
						component2.dmName = profile.name;
						component2.chat = profile.dms;
					}
				});
			}
		}));
	}

	public void CleanUI()
	{
		foreach (Transform child in Container)
		{
			Object.Destroy(child.gameObject);
		}
	}

	public static async void LoadPfp(string steamid, Image img)
	{
		if (ulong.TryParse(steamid, out var result))
		{
			img.sprite = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
		}
	}
}
