using System.Collections.Generic;
using HarmonyLib;
using UltraNet.Canvas;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Patches;

[HarmonyPatch(typeof(FinalCyberRank))]
public static class FinalCyberRankPatch
{
	public const string url = "https://duviz.xyz/ultranet/user/completeCybergrind";

	public static int rankScore;

	public static float lastTime;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FinalCyberRank), "GameOver")]
	public static void GameOver(FinalCyberRank __instance)
	{
		if (!PlayerFetcher.CheatsActive() && LeaderboardController.CanSubmitScores && !(lastTime + 30f > Time.unscaledTime))
		{
			lastTime = Time.unscaledTime;
			Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/user/completeCybergrind", new Dictionary<string, string>
			{
				{
					"token",
					ContentManager.GetToken()
				},
				{
					"wave",
					Mathf.ClampToInt((long)__instance.savedWaves).ToString()
				}
			}, delegate
			{
			}));
		}
	}
}
