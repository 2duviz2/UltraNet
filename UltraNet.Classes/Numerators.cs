using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UltraNet.Classes;

public class Numerators : MonoBehaviour
{
	public static Numerators instance;

	public static bool _busy;

	public void Awake()
	{
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		instance = this;
	}

	public IEnumerator TimerButton(Button button, float time)
	{
		yield return new WaitForSecondsRealtime(time);
		if (!(button == null))
		{
			if (!_busy)
			{
				button.onClick.Invoke();
			}
			StartCoroutine(TimerButton(button, time));
		}
	}

	public static IEnumerator GetStringFromUrl(string url, Action<string> callback)
	{
		using UnityWebRequest www = UnityWebRequest.Get(url);
		www.timeout = 10;
		yield return www.SendWebRequest();
		if (www.result != UnityWebRequest.Result.Success)
		{
			if (!string.IsNullOrEmpty(www.error) && www.error.Contains("Unknown Error"))
			{
				callback?.Invoke("?");
			}
			Plugin.LogError("Failed to load string: " + www.error);
			callback?.Invoke(null);
		}
		else
		{
			callback?.Invoke(www.downloadHandler.text);
		}
	}

	public static IEnumerator PostRequest(string url, Dictionary<string, string> postData, Action<string> callback)
	{
		_ = Time.realtimeSinceStartup;
		WWWForm form = new WWWForm();
		foreach (KeyValuePair<string, string> pair in postData)
		{
			form.AddField(pair.Key, pair.Value);
		}
		using UnityWebRequest www = UnityWebRequest.Post(url, form);
		www.timeout = 10;
		yield return www.SendWebRequest();
		if (www.result != UnityWebRequest.Result.Success)
		{
			if (!string.IsNullOrEmpty(www.error) && www.error.Contains("Unknown Error"))
			{
				callback?.Invoke("?");
			}
			Plugin.LogError("Failed to post request: " + www.error);
			callback?.Invoke(null);
		}
		else
		{
			callback?.Invoke(www.downloadHandler.text);
		}
	}
}
