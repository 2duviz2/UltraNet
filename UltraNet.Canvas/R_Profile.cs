using System.Collections.Generic;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

public class R_Profile : MonoBehaviour
{
	public TMP_InputField Name;

	public TMP_InputField Bio;

	public TMP_InputField ID;

	public TMP_InputField Pronouns;

	public TMP_InputField Status;

	public TMP_InputField SearchField;

	public Image Pfp;

	public GameObject ProfileSettings;

	public GameObject SendFriendRequestButton;

	public GameObject RemoveFriendButton;

	private bool loading = false;

	private float timer = 0f;

	private string id;

	public void LoadProfile(string steamid)
	{
		if (loading)
		{
			return;
		}
		id = steamid;
		loading = true;
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/profileV2", new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "steamid", steamid }
		}, delegate(string json)
		{
			loading = false;
			List<Profile> list = ProfileParser.Parse(json);
			if (list.Count >= 1)
			{
				Profile profile = list[0];
				Name.text = profile.name + " - " + profile.pronouns;
				Bio.text = profile.description;
				Pronouns.text = profile.permissions;
				ID.text = "#" + profile.id2;
				Status.text = profile.status;
				LoadPfp(profile.id);
				ProfileSettings.SetActive(profile.id == ContentManager.steamid);
				SendFriendRequestButton.SetActive(profile.id != ContentManager.steamid && profile.friends.ToLower() != "true");
				RemoveFriendButton.SetActive(profile.id != ContentManager.steamid && profile.friends.ToLower() == "true");
			}
			else
			{
				id = "";
				Name.text = "User not found";
				Bio.text = "<size=0>";
				Pronouns.text = "<size=0>";
				ID.text = "<size=0>";
				Status.text = "<size=0>";
				Pfp.sprite = null;
				ProfileSettings.SetActive(value: false);
				SendFriendRequestButton.SetActive(value: false);
			}
		}));
	}

	public void SetAsLoading()
	{
		ProfileSettings.SetActive(value: false);
		SendFriendRequestButton.SetActive(value: false);
		Name.text = "Loading...";
		Bio.text = "Loading...";
		Pronouns.text = "Loading...";
		ID.text = "Loading...";
		Status.text = "Loading...";
		Pfp.sprite = null;
	}

	public void Search()
	{
		if (SearchField.text != "")
		{
			SetAsLoading();
			LoadProfile(SearchField.text);
		}
		else
		{
			SetAsLoading();
			LoadProfile(ContentManager.steamid);
		}
	}

	public void SendFriendRequest()
	{
		SendFriendRequestButton.SetActive(value: false);
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/sendFriendRequest", new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "steamid", id }
		}, delegate
		{
		}));
	}

	public void RemoveFriend()
	{
		RemoveFriendButton.SetActive(value: false);
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/setFriends", new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "steamid", id },
			{ "status", "false" }
		}, delegate
		{
		}));
	}

	public async void LoadPfp(string steamid)
	{
		if (ulong.TryParse(steamid, out var result))
		{
			Sprite img = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
			Pfp.sprite = img;
		}
	}

	public void Update()
	{
		if (!loading && !(id == ""))
		{
			timer += Time.unscaledDeltaTime;
			if (timer >= 7f)
			{
				timer = 0f;
				LoadProfile(id);
			}
		}
	}
}
