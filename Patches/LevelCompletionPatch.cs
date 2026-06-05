//namespace UltraNet.Patches;

//using HarmonyLib;
//using System;
//using UltraNet.Canvas;
//using UltraNet.Classes;
//using UnityEngine;
//using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

//[HarmonyPatch(typeof(FinalPit))]
//public static class LevelCompletionPatch
//{
//    public const string url = "https://duviz.xyz/ultranet/user/complete";
//    public static int rankScore = 0;

//    public static float lastTime = 0;

//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(FinalPit), nameof(FinalPit.OnTriggerEnter))]
//    public static void OnTriggerEnter(FinalPit __instance)
//    {
//        if (PlayerFetcher.CheatsActive()) return;
//        if (lastTime + 10f > Time.unscaledTime) return;
//        lastTime = Time.unscaledTime;

//        float totalSeconds = StatsManager.Instance.seconds;

//        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
//        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
//        int milliseconds = Mathf.FloorToInt((totalSeconds * 1000f) % 1000f);

//        string time = $"{minutes:00}:{seconds:00}:{milliseconds:000}";

//        Numerators.instance.StartCoroutine(Numerators.PostRequest(url, new() {
//            { "token", ContentManager.GetToken() },
//            { "level", SceneHelper.CurrentScene },
//            { "time", time },
//            { "rank", GetRank() },
//        }, (json) =>
//        {

//        }));
//    }

//    public static string GetRank()
//    {
//        rankScore = 0;

//        var sm = StatsManager.Instance;

//        GetRanks(sm.timeRanks, sm.seconds, true, true);
//        GetRanks(sm.killRanks, (float)sm.kills, false, true);
//        GetRanks(sm.styleRanks, (float)sm.stylePoints, false, true);

//        var currentScene = SceneHelper.CurrentScene;

//        var diff = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0);

//        if (rankScore == 12 && sm.restarts == 0)
//        {
//            return "P";
//        }

//        return GetRankName((float)rankScore / 3f);
//    }

//    public static string GetRankName(float score)
//    {
//        var score2 = Mathf.RoundToInt(score);

//        return score2 switch
//        {
//            1 => "C",
//            2 => "B",
//            3 => "A",
//            4 or 5 or 6 => "S",
//            _ => "D",
//        };
//    }

//    public static string GetRanks(int[] ranksToCheck, float value, bool reverse, bool addToRankScore = false)
//    {
//        int num = 0;
//        bool flag = true;
//        while (flag)
//        {
//            if (num >= ranksToCheck.Length)
//            {
//                if (addToRankScore)
//                {
//                    rankScore += 4;
//                }
//                return "<color=#FF0000>S</color>";
//            }
//            if ((reverse && value <= (float)ranksToCheck[num]) || (!reverse && value >= (float)ranksToCheck[num]))
//            {
//                num++;
//            }
//            else
//            {
//                if (addToRankScore)
//                {
//                    rankScore += num;
//                }
//                switch (num)
//                {
//                    case 0:
//                        return "<color=#0094FF>D</color>";
//                    case 1:
//                        return "<color=#4CFF00>C</color>";
//                    case 2:
//                        return "<color=#FFD800>B</color>";
//                    case 3:
//                        return "<color=#FF6A00>A</color>";
//                }
//            }
//        }
//        return "X";
//    }
//}