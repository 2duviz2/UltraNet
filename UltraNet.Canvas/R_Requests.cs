using System.Collections.Generic;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

public class R_Requests : MonoBehaviour
{
	public TMP_InputField ID;

	public TMP_InputField Field;

	public Transform Container;

	public GameObject RequestItem;

	private bool loading = false;

	private float timer = 0f;

	public void Start()
	{
		ID.text = "ID: #" + ContentManager.compressedid;
		Fetch();
	}

	public void Update()
	{
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
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/getFriendRequests", new Dictionary<string, string> { 
		{
			"token",
			ContentManager.GetToken()
		} }, delegate(string json)
		{
			loading = false;
			List<Profile> list = ProfileParser.Parse(json);
			CleanUI();
			foreach (Profile profile in list)
			{
				GameObject item = Object.Instantiate(RequestItem, Container);
				LoadPfp(profile.id, item.transform.GetChild(0).GetChild(0).GetComponent<Image>());
				item.transform.GetChild(1).GetComponent<TMP_InputField>().text = profile.name;
				GameObject gameObject = item.transform.GetChild(0).gameObject;
				GameObject gameObject2 = item.transform.GetChild(2).gameObject;
				GameObject gameObject3 = item.transform.GetChild(3).gameObject;
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					R_DMs.OpenProfileStatic(profile.id);
				});
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					item.SetActive(value: false);
					Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/setFriends", new Dictionary<string, string>
					{
						{
							"token",
							ContentManager.GetToken()
						},
						{ "steamid", profile.id },
						{ "status", "true" }
					}, delegate
					{
					}));
				});
				gameObject3.GetComponent<Button>().onClick.AddListener(delegate
				{
					item.SetActive(value: false);
					Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/setFriends", new Dictionary<string, string>
					{
						{
							"token",
							ContentManager.GetToken()
						},
						{ "steamid", profile.id },
						{ "status", "false" }
					}, delegate
					{
					}));
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

	public async void LoadPfp(string steamid, Image img)
	{
		if (ulong.TryParse(steamid, out var result))
		{
			img.sprite = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
		}
	}

	public void Search()
	{
		if (!string.IsNullOrEmpty(Field.text))
		{
			R_DMs.OpenProfileStatic(Field.text);
		}
	}

	public void CopyID()
	{
		GUIUtility.systemCopyBuffer = ContentManager.compressedid;
	}
}
