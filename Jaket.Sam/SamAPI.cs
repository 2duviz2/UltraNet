using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Jaket.Sam;

public class SamAPI
{
	public static Sam Sam = new Sam();

	public static float[] Say(string text)
	{
		text += "[";
		Sam.Text2Phonemes(text, out var output);
		Sam.SetInput(output);
		return Sam.GetBuffer().GetFloats();
	}

	public static AudioClip Clip(string text)
	{
		float[] data = Say(text);
		AudioClip clip = AudioClip.Create("Sam", data.Length, 1, 22050, stream: false);
		clip.SetData(data, 0);
		return clip;
	}

	public static string CutColors(string original)
	{
		return Regex.Replace(original, "<.*?>|\\[.*?\\]", string.Empty);
	}

	public static void TryPlay(string text, AudioSource source)
	{
		try
		{
			source.clip = Clip(CutColors(text));
			source.Play();
		}
		catch (Exception)
		{
		}
	}
}
