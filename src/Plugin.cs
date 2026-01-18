namespace YourModNameHere;

using BepInEx;
using HarmonyLib;

/// <summary> General plugin class. </summary>
[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    /// <summary> Load the mod. </summary>
    public void Awake()
    {
        // load code or whatever
        new Harmony(PluginInfo.GUID).PatchAll();
    }
}

public class PluginInfo
{
    public const string GUID = "YourName.YourModName";
    public const string Name = "YourModNameHere";
    public const string Version = "1.0.0";
}