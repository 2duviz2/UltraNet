using TMPro;
using UltraNet.Canvas;
using UnityEngine;

namespace UltraNet.Classes;

public class Player : MonoBehaviour
{
	public float lerpSpeed = 2f;

	private Vector3 targetPos;

	private Vector3Lerp lerpPos;

	private TMP_Text t;

	private string originalText;

	private bool lastCheats = false;

	public void SetTarget(Vector3 newPos, bool cheats)
	{
		targetPos = newPos;
		lerpPos.Set(newPos);
		if (lastCheats != cheats)
		{
			t.text = (cheats ? ("<size=0.5>(C)</size>" + originalText) : originalText);
		}
		lastCheats = cheats;
	}

	public void CreateName(string text, string id, string url)
	{
		GameObject playerName = Object.Instantiate(BundlesManager.netBundle.LoadAsset<GameObject>("PlayerName"));
		playerName.transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
		playerName.transform.SetParent(base.transform, worldPositionStays: false);
		playerName.GetComponentInChildren<TMP_Text>().text = TextParser.Parse(text);
		t = playerName.GetComponentInChildren<TMP_Text>();
		originalText = TextParser.Parse(text);
		GetPfp(id, playerName, url);
	}

	public async void GetPfp(string id, GameObject obj, string url)
	{
		if (!string.IsNullOrEmpty(url))
		{
			obj.GetComponentInChildren<ImageGetter>().imageUrl = url;
			obj.GetComponentInChildren<ImageGetter>().SetImg();
		}
	}

	public void Update()
	{
		base.transform.position = lerpPos.Grab();
		base.transform.LookAt((Camera.main != null) ? Camera.main.transform.position : Vector3.zero);
	}
}
