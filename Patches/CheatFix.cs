using HarmonyLib;

namespace UltraNet.Patches
{
    [HarmonyPatch]
    public class CheatFix
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.HandleCheatBind))]
        static bool Patch1() => !Plugin.UIBusy();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CheatsController), nameof(CheatsController.Update))]
        static bool Patch2() => !Plugin.UIBusy();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ULTRAKILL.Cheats.Noclip), "UpdateTick")]
        static bool Patch3() => !Plugin.UIBusy();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ULTRAKILL.Cheats.Flight), "Update")]
        static bool Patch4() => !Plugin.UIBusy();
    }
}