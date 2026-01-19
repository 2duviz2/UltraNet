namespace UltraNet;

using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;

    GameObject canvasObject;
    GameObject canvasInstance;

    public void Awake()
    {
        hideFlags = UnityEngine.HideFlags.HideAndDontSave;
        instance = this;

        new Harmony(PluginInfo.GUID).PatchAll();


        LogInfo("UltraNet loaded.");
    }

    public void Start()
    {
        GameObject obj = new GameObject("BundlesManager");
        obj.AddComponent<BundlesManager>();
        canvasObject = BundlesManager.netBundle.LoadAsset<GameObject>("UltraNetCanvas");
        canvasInstance = Instantiate(canvasObject);
        canvasInstance.SetActive(false);
        DontDestroyOnLoad(canvasInstance);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
                return;
            canvasInstance.SetActive(!canvasInstance.activeSelf);
        }
    }

    public static void LogInfo(object msg) { instance.Logger.LogInfo(msg); }
    public static void LogWarning(object msg) { instance.Logger.LogWarning(msg); }
    public static void LogError(object msg) { instance.Logger.LogError(msg); }
}

public class PluginInfo
{
    public const string GUID = "duviz.UltraNet";
    public const string Name = "UltraNet";
    public const string Version = "0.0.1";
}