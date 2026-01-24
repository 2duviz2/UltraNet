namespace UltraNet;

using BepInEx;
using HarmonyLib;
using TMPro;
using UltraNet.Canvas;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.EventSystems;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;
    public static bool debugMode = false;

    GameObject canvasObject;
    GameObject canvasInstance;

    bool lastPlayerActive = false;
    bool openedOnce = false;

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
        GameObject playerFetcher = new GameObject("PlayerFetcher");

        obj.AddComponent<BundlesManager>();
        playerFetcher.AddComponent<PlayerFetcher>();
        gameObject.AddComponent<Numerators>();

        GameObject notifications = Instantiate(BundlesManager.netBundle.LoadAsset<GameObject>("UltraNetNotifications"));
        canvasObject = BundlesManager.netBundle.LoadAsset<GameObject>("UltraNetCanvas");
        canvasInstance = Instantiate(canvasObject);
        canvasInstance.SetActive(false);

        DontDestroyOnLoad(canvasInstance);
        DontDestroyOnLoad(playerFetcher);

        openedOnce = PlayerPrefs.GetInt("UltraNet_Opened", 0) == 1;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null && EventSystem.current.currentSelectedGameObject.GetComponent<GayInputField>() != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused && EventSystem.current.currentSelectedGameObject.activeInHierarchy)
                return;
            canvasInstance.SetActive(!canvasInstance.activeSelf);
            if (!openedOnce)
            {
                PlayerPrefs.SetInt("UltraNet_Opened", 1);
                openedOnce = true;
            }
        }

        if (NewMovement.Instance != null && !openedOnce)
        {
            if (lastPlayerActive != NewMovement.Instance.activated)
            {
                HudMessageReceiver.Instance.SendHudMessage("Press <color=#ff66cc>(T)</color> to open <color=#66ff66>UltraNet</color>");
            }
            lastPlayerActive = NewMovement.Instance.activated;
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