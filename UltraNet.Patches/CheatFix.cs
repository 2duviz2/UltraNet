using HarmonyLib;
using ULTRAKILL.Cheats;

namespace UltraNet.Patches;

[HarmonyPatch]
public class CheatFix
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(CheatsManager), "HandleCheatBind")]
	private static bool Patch1()
	{
		return !Plugin.UIBusy();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CheatsController), "Update")]
	private static bool Patch2()
	{
		return !Plugin.UIBusy();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Noclip), "UpdateTick")]
	private static bool Patch3()
	{
		return !Plugin.UIBusy();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Flight), "Update")]
	private static bool Patch4()
	{
		return !Plugin.UIBusy();
	}
}
