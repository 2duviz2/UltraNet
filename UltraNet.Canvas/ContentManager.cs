using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Canvas;

public class ContentManager : MonoBehaviour
{
	[Serializable]
	public class R_Window
	{
		public string id;

		public GameObject prefab;

		public R_Window(string id, GameObject prefab)
		{
			this.id = id;
			this.prefab = prefab;
		}
	}

	public static ContentManager instance;

	public static List<string> openWindows = new List<string>();

	public static List<GameObject> openWindowsObjs = new List<GameObject>();

	public const string mainUrl = "D";

	public const string loginUrl = "https://duviz.xyz/login";

	public const string chatUrl = "https://duviz.xyz/ultranet/chat";

	public const string sendChatUrl = "https://duviz.xyz/ultranet/chat/sendMessageV2";

	public const string profileUrl = "https://duviz.xyz/ultranet/profileV2";

	public const string sendFriendRequestUrl = "https://duviz.xyz/ultranet/sendFriendRequest";

	public const string setFriendsUrl = "https://duviz.xyz/ultranet/setFriends";

	public const string getFriendsUrl = "https://duviz.xyz/ultranet/getFriends";

	public const string getFriendRequestsUrl = "https://duviz.xyz/ultranet/getFriendRequests";

	public const string getNotificationsUrl = "https://duviz.xyz/ultranet/getNotifications";

	public static string steamid = "";

	public static string compressedid = "";

	public static TMP_InputField CurrentChatField;

	[Header("Prefabs")]
	public List<R_Window> windows = new List<R_Window>();

	public AudioSource source = null;

	private bool checking = false;

	public static string profileSetBioUrl => "https://duviz.xyz/ultranet/user/" + steamid + "/editBio";

	public static string profileSetPronounsUrl => "https://duviz.xyz/ultranet/user/" + steamid + "/editPron";

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		source = new GameObject("Audio").AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.volume = 1f;
		UnityEngine.Object.DontDestroyOnLoad(source.gameObject);
		MonoBehaviour.print(windows.Count);
		MonoBehaviour.print(windows);
		foreach (R_Window w in windows)
		{
			Plugin.LogInfo($"Window: {w.id}, isnull: {w.prefab == null}");
		}
		if (windows.Count == 0)
		{
			Plugin.LogWarning("Windows list is empty, loading from bundles...");
			windows.Add(new R_Window("login", BundlesManager.netBundle.LoadAsset<GameObject>("Login Variant")));
			windows.Add(new R_Window("main", BundlesManager.netBundle.LoadAsset<GameObject>("UltranetMain Variant")));
			windows.Add(new R_Window("rules", BundlesManager.netBundle.LoadAsset<GameObject>("Rules Variant")));
			windows.Add(new R_Window("chat", BundlesManager.netBundle.LoadAsset<GameObject>("Chat Variant")));
			windows.Add(new R_Window("profile", BundlesManager.netBundle.LoadAsset<GameObject>("Profile Variant")));
			windows.Add(new R_Window("profilesettings", BundlesManager.netBundle.LoadAsset<GameObject>("Profile Settings Variant")));
			windows.Add(new R_Window("friends", BundlesManager.netBundle.LoadAsset<GameObject>("Friends Variant")));
			windows.Add(new R_Window("requests", BundlesManager.netBundle.LoadAsset<GameObject>("FriendRequests Variant")));
			windows.Add(new R_Window("friendChat", BundlesManager.netBundle.LoadAsset<GameObject>("Friend Chat Variant")));
		}
	}

	public void CheckForLogin()
	{
		if (checking)
		{
			return;
		}
		checking = true;
		if (GetToken() == "")
		{
			SpawnWindow("login");
			checking = false;
			return;
		}
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/login", new Dictionary<string, string> { 
		{
			"token",
			GetToken()
		} }, delegate(string json)
		{
			checking = false;
			if (json.StartsWith("Yay!"))
			{
				string text = json.Replace("Yay!", "");
				steamid = text.Split('#')[0];
				compressedid = text.Split('#')[1];
				SpawnWindow("main");
			}
			else
			{
				SpawnWindow("login");
			}
		}));
	}

	public void Open()
	{
		CurrentChatField?.ActivateInputField();
	}

	public void ClearWindows()
	{
		foreach (GameObject win in openWindowsObjs.ToList())
		{
			if (win != null)
			{
				UnityEngine.Object.Destroy(win);
			}
		}
		openWindows = new List<string>();
		openWindowsObjs = new List<GameObject>();
	}

	public void Login(string key)
	{
		if (key == "")
		{
			key = GUIUtility.systemCopyBuffer;
		}
		PlayerPrefs.SetString("UltranetToken", key);
		ClearWindows();
	}

	public void OpenProfile(string steamid)
	{
		GameObject w = SpawnWindow("profile");
		if ((bool)w)
		{
			w.GetComponent<R_Profile>().LoadProfile(steamid);
		}
	}

	public GameObject SpawnWindow(string id)
	{
		return SpawnWindowCustomID(id, id);
	}

	public GameObject SpawnWindowCustomID(string id, string customID)
	{
		R_Window w = windows.Where((R_Window r_Window) => r_Window.id == id).FirstOrDefault();
		if (w != null)
		{
			if (openWindows.Contains(customID))
			{
				Plugin.LogError("Window with id '" + customID + "' is already open!");
				return null;
			}
			Plugin.LogInfo("Spawning window '" + id + "' with custom id '" + customID + "'");
			GameObject ww = UnityEngine.Object.Instantiate(w.prefab, base.transform.GetChild(1));
			ww.name = customID;
			return ww;
		}
		Plugin.LogError("No window with id '" + id + "' found!");
		return null;
	}

	public void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance != null && SceneHelper.CurrentScene != "Main Menu" && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
		Time.timeScale = 0f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Time.timeScale = 1f;
			base.gameObject.SetActive(value: false);
			if (MonoSingleton<OptionsManager>.Instance != null && SceneHelper.CurrentScene != "Main Menu")
			{
				MonoSingleton<OptionsManager>.Instance.UnPause();
			}
		}
		if (openWindows.Count == 0)
		{
			CheckForLogin();
		}
	}

	public Color ParseColor(string colorStr)
	{
		if (ColorUtility.TryParseHtmlString(colorStr, out var color))
		{
			return color;
		}
		return Color.white;
	}

	public static string GetToken()
	{
		return PlayerPrefs.GetString("UltranetToken", "");
	}

	public static string GetPosition()
	{
		if (Camera.main == null)
		{
			return Vector3.zero.ToString();
		}
		return Camera.main.transform.position.ToString();
	}
}
