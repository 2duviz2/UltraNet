using System.Collections.Generic;
using TMPro;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Canvas;

public class R_ProfileSettings : MonoBehaviour
{
	public TMP_InputField bio;

	public TMP_InputField pron;

	public void UpdateBio()
	{
		Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileSetBioUrl, new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "input", bio.text }
		}, delegate
		{
		}));
	}

	public void UpdatePronouns()
	{
		Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileSetPronounsUrl, new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{ "input", pron.text }
		}, delegate
		{
		}));
	}
}
