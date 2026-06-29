using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraNet.Canvas;

public static class ProfileParser
{
	public static List<Profile> Parse(string json)
	{
		try
		{
			string wrapped = "{ \"profiles\": " + json + "}";
			ProfileList result = JsonUtility.FromJson<ProfileList>(wrapped);
			return result.profiles;
		}
		catch (Exception)
		{
			return new List<Profile>();
		}
	}
}
