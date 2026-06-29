using BepInEx;
using HarmonyLib;
using TMPro;
using UltraNet.Canvas;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraNet;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
	public static Plugin instance;

	public static bool debugMode;

	private GameObject canvasObject;

	private GameObject canvasInstance;

	private bool lastPlayerActive = false;

	private bool openedOnce = false;

	public static TMP_SpriteAsset defaultSpriteAsset;

	public void Awake()
	{
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		instance = this;
		new Harmony("duviz.UltraNet").PatchAll();
		LogInfo("UltraNet loaded.");
	}

	public void Start()
	{
		GameObject obj = new GameObject("Managers")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		GameObject playerFetcher = new GameObject("PlayerFetcher")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		obj.AddComponent<BundlesManager>();
		obj.AddComponent<Numerators>();
		obj.AddComponent<CustomBindingsPoCPlugin.InputListenerInstance>();
		playerFetcher.AddComponent<PlayerFetcher>();
		GameObject notifications = Object.Instantiate(BundlesManager.netBundle.LoadAsset<GameObject>("UltraNetNotifications"));
		canvasObject = BundlesManager.netBundle.LoadAsset<GameObject>("UltraNetCanvas");
		canvasInstance = Object.Instantiate(canvasObject);
		canvasInstance.SetActive(value: false);
		canvasInstance.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
		defaultSpriteAsset = BundlesManager.netBundle.LoadAsset<TMP_SpriteAsset>("ilovemen");
		Object.DontDestroyOnLoad(canvasInstance);
		Object.DontDestroyOnLoad(playerFetcher);
		Object.DontDestroyOnLoad(obj);
		openedOnce = PlayerPrefs.GetInt("UltraNet_Opened", 0) == 1;
		SceneManager.sceneLoaded += SceneLoadDelayed;
	}

	public void SceneLoadDelayed(Scene _, LoadSceneMode __)
	{
		Invoke("SceneLoad", 0.1f);
	}

	public void SceneLoad()
	{
		if (SceneHelper.CurrentScene == "Main Menu" && base.gameObject.GetComponent<CustomBindingsPoCPlugin.InputListener>() == null)
		{
			base.gameObject.AddComponent<CustomBindingsPoCPlugin.InputListener>();
		}
	}

	public void Update()
	{
		if (MonoSingleton<NewMovement>.Instance != null && !openedOnce)
		{
			if (lastPlayerActive != MonoSingleton<NewMovement>.Instance.activated)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Press <color=#ff66cc>(T)</color> to open <color=#66ff66>UltraNet</color>");
			}
			lastPlayerActive = MonoSingleton<NewMovement>.Instance.activated;
		}
	}

	public void PressKey()
	{
		if (!UIBusy())
		{
			canvasInstance.SetActive(!canvasInstance.activeSelf);
			if (canvasInstance.activeSelf)
			{
				ContentManager.instance.Open();
			}
			if (!openedOnce)
			{
				PlayerPrefs.SetInt("UltraNet_Opened", 1);
				openedOnce = true;
			}
		}
	}

	public static bool UIBusy()
	{
		return EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused && EventSystem.current.currentSelectedGameObject.activeInHierarchy;
	}

	public void OnApplicationFocus(bool isFocused)
	{
		if (MonoSingleton<OptionsManager>.Instance != null && SceneHelper.CurrentScene != "Main Menu" && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
	}

	public static T Ass<T>(string path)
	{
		return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
	}

	public static void LogInfo(object msg)
	{
		instance.Logger.LogInfo(msg);
	}

	public static void LogWarning(object msg)
	{
		instance.Logger.LogWarning(msg);
	}

	public static void LogError(object msg)
	{
		instance.Logger.LogError(msg);
	}
}
