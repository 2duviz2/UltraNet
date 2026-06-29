using System.Collections.Generic;
using HarmonyLib;
using UltraNet.Canvas;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Patches;

[HarmonyPatch(typeof(FinalPit))]
public static class LevelCompletionPatch
{
	public const string url = "https://duviz.xyz/ultranet/user/complete";

	public static int rankScore;

	public static float lastTime;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FinalPit), "SendInfo")]
	public static void SendInfo(FinalPit __instance)
	{
		if (!__instance.infoSent && !PlayerFetcher.CheatsActive() && LeaderboardController.CanSubmitScores && !(lastTime + 30f > Time.unscaledTime))
		{
			lastTime = Time.unscaledTime;
			float totalSeconds = MonoSingleton<StatsManager>.Instance.seconds;
			int minutes = Mathf.FloorToInt(totalSeconds / 60f);
			int seconds = Mathf.FloorToInt(totalSeconds % 60f);
			int milliseconds = Mathf.FloorToInt(totalSeconds * 1000f % 1000f);
			string time = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
			Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/user/complete", new Dictionary<string, string>
			{
				{
					"token",
					ContentManager.GetToken()
				},
				{
					"level",
					SceneHelper.CurrentScene
				},
				{ "time", time },
				{
					"rank",
					GetRank()
				}
			}, delegate
			{
			}));
		}
	}

	public static string GetRank()
	{
		rankScore = 0;
		StatsManager sm = MonoSingleton<StatsManager>.Instance;
		GetRanks(sm.timeRanks, sm.seconds, reverse: true, addToRankScore: true);
		GetRanks(sm.killRanks, sm.kills, reverse: false, addToRankScore: true);
		GetRanks(sm.styleRanks, sm.stylePoints, reverse: false, addToRankScore: true);
		string currentScene = SceneHelper.CurrentScene;
		int diff = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (rankScore == 12 && sm.restarts == 0)
		{
			return "P";
		}
		return GetRankName((float)rankScore / 3f);
	}

	public static string GetRankName(float score)
	{
		int score2 = Mathf.RoundToInt(score);
		if (1 == 0)
		{
		}
		string result;
		switch (score2)
		{
		case 1:
			result = "C";
			break;
		case 2:
			result = "B";
			break;
		case 3:
			result = "A";
			break;
		case 4:
		case 5:
		case 6:
			result = "S";
			break;
		default:
			result = "D";
			break;
		}
		if (1 == 0)
		{
		}
		return result;
	}

	public static string GetRanks(int[] ranksToCheck, float value, bool reverse, bool addToRankScore = false)
	{
		int num = 0;
		bool flag = true;
		while (flag)
		{
			if (num >= ranksToCheck.Length)
			{
				if (addToRankScore)
				{
					rankScore += 4;
				}
				return "<color=#FF0000>S</color>";
			}
			if ((reverse && value <= (float)ranksToCheck[num]) || (!reverse && value >= (float)ranksToCheck[num]))
			{
				num++;
				continue;
			}
			if (addToRankScore)
			{
				rankScore += num;
			}
			switch (num)
			{
			case 0:
				return "<color=#0094FF>D</color>";
			case 1:
				return "<color=#4CFF00>C</color>";
			case 2:
				return "<color=#FFD800>B</color>";
			case 3:
				return "<color=#FF6A00>A</color>";
			}
		}
		return "X";
	}
}
