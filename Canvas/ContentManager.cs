using Jaket.Sam;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class ContentManager : MonoBehaviour
    {
        public Transform content;
        public ResetScrollbar resetScrollbar;
        public ScrollRect scrollRect;

        [Header("Root")]
        public TMP_Text titleText;

        [Header("Prefabs")]
        public GameObject textPrefab;
        public GameObject imagePrefab;
        public GameObject buttonPrefab;
        public GameObject inputFieldPrefab;
        public GameObject playerPrefab;

        AudioSource source = null;

        public const string mainUrl = "https://duviz.xyz/login";
        public const string errorJson = @"
            {
                'title': 'Error!',
                'elements': [
                    {
                        'type': 'text',
                        'name': 'ErrorText',
                        'text': 'There was an error when trying to load the site. Try again later.',
                        'color': '#FF0000'
                    },
                    {
                        'type': 'button',
                        'name': 'BackButton',
                        'text': 'Go back',
                        'url': 'https://duviz.xyz/login',
                        'action': 'postToken'
                    }
                ]
            }
        ";
        public string lastJson = "";

        public void Start()
        {
            transform.GetChild(1).GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
            PostWebsite(mainUrl, new Dictionary<string, string> { { "token", GetToken() } });

            source = new GameObject("Audio").AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 1f;
            DontDestroyOnLoad(source.gameObject);
        }

        public void Update()
        {
            OptionsManager.Instance.Pause();
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 1f;
                gameObject.SetActive(false);
                OptionsManager.Instance.UnPause();
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                lastJson = "";
                PostWebsite(mainUrl, new Dictionary<string, string> { { "token", GetToken() } });
            }
        }

        public void OnEnable()
        {
            if (titleText.text.Contains("LOADING"))
            {
                lastJson = "";
                PostWebsite(mainUrl, new Dictionary<string, string> { { "token", GetToken() } });
            }
            else
            {
                if (titleText.text.Contains("CHAT"))
                {
                    string lj = lastJson;
                    lastJson = "";
                    ParseJson(lj);
                }
            }
        }

        public void LoadWebsite(string url, bool deletePrev = true)
        {
            if (deletePrev)
                Numerators.instance.StopAllCoroutines();
            if (deletePrev) { CleanUp(); lastJson = null; }
            Numerators.instance.StartCoroutine(Numerators.GetStringFromUrl(url, (json) =>
            {
                if (json != null)
                {
                    if (json == "?")
                        return;
                    ParseJson(json);
                }
                else
                {
                    ParseJson(errorJson);
                }
            }));
        }

        public void PostWebsite(string url, Dictionary<string, string> postData,  bool deletePrev = true)
        {
            if (deletePrev)
                Numerators.instance.StopAllCoroutines();
            if (deletePrev) { CleanUp(); lastJson = null; }
            Numerators.instance.StartCoroutine(Numerators.PostRequest(url, postData, (json) =>
            {
                if (json != null)
                {
                    if (json == "?")
                        return;
                    ParseJson(json);
                }
                else
                {
                    if (deletePrev)
                        ParseJson(errorJson);
                }
            }));
        }

        string lastTTS = "";
        public void ParseJson(string json)
        {
            if (lastJson == json) return;
            if (!gameObject.activeInHierarchy) NotificationListener.Show();
            string lastTitle = titleText.text;
            CleanUp();

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Failed to parse scene json: {ex}");
                ParseJson(errorJson);
                return;
            }

            titleText.text = (root["title"]?.ToString() ?? "Unnamed").ToUpper();

            if (root["cookieKey"] != null)
            {
                string cookieKey = root["cookieKey"]?.ToString() ?? "Element";
                string cookieValue = root["cookieValue"]?.ToString() ?? "Element";
                PlayerPrefs.SetString(cookieKey, cookieValue);
            }

            if (root["tts"] != null)
            {
                if (lastTTS != root["tts"].ToString())
                {
                    source.volume = PlayerPrefs.GetFloat("UltranetConfig_TTSVolume", 0.5f);
                    lastTTS = root["tts"].ToString();
                    SamAPI.TryPlay(root["tts"].ToString(), source);
                }
            }


            List<(string, TMP_InputField)> inputFields = [];
            foreach (var element in root["elements"])
            {
                string type = element["type"]?.ToString();
                GameObject prefab = type switch
                {
                    "text" => textPrefab,
                    "image" => imagePrefab,
                    "button" => buttonPrefab,
                    "inputField" => inputFieldPrefab,
                    "player" => playerPrefab,
                    _ => null
                };
                if (prefab == null)
                {
                    Plugin.LogWarning($"Unknown element type: {type}");
                    continue;
                }
                GameObject obj = Instantiate(prefab, content);
                obj.name = element["name"]?.ToString() ?? "Element";

                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(
                    element["width"] != null ? (float)element["width"] : rectTransform.sizeDelta.x,
                    element["height"] != null ? (float)element["height"] : rectTransform.sizeDelta.y
                );

                if (Plugin.debugMode)
                {
                    GameObject idText = Instantiate(textPrefab, obj.transform);
                    idText.GetComponent<RectTransform>().localPosition = new Vector3(600, 0, 0);
                    TMP_Text idTextComp = idText.GetComponent<TMP_Text>();
                    idTextComp.alignment = TextAlignmentOptions.Left;
                    idTextComp.text = $"#{obj.name}";
                    idTextComp.enableAutoSizing = false;
                    idTextComp.fontSize = 16;
                    idTextComp.color = new Color(1, 1, 1, 1f);
                    idTextComp.raycastTarget = false;
                }

                // Set specific properties
                switch (type)
                {
                    case "text":
                        var textComp = obj.GetComponent<TMP_Text>();
                        if (textComp != null)

                        {
                            textComp.text = TextParser.Parse(element["text"]?.ToString()) ?? "Text";
                            if (element["color"] != null)
                                textComp.color = ParseColor(element["color"]?.ToString());
                            textComp.alignment = element["alignment"] != null ? (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), element["alignment"].ToString()) : TextAlignmentOptions.Center;
                        }
                        break;
                    case "image":
                        var imageComp = obj.transform.GetComponent<ImageGetter>();
                        if (imageComp != null)
                        {
                            string url = element["url"]?.ToString();
                            imageComp.imageUrl = url;
                            imageComp.SetImg();
                        }
                        break;
                    case "button":
                        var buttonComp = obj.GetComponent<Button>();
                        var buttonTextComp = obj.GetComponentInChildren<TMP_Text>();
                        if (buttonTextComp != null)
                        {
                            buttonTextComp.text = TextParser.Parse(element["text"]?.ToString()) ?? "Button";
                            buttonTextComp.alignment = element["alignment"] != null ? (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), element["alignment"].ToString()) : TextAlignmentOptions.Center;
                            if (element["color"] != null)
                                buttonTextComp.color = ParseColor(element["color"]?.ToString());
                            
                            string url = element["url"]?.ToString();
                            string action = element["action"]?.ToString();
                            string inputFieldName = element["inputFieldName"]?.ToString();
                            string copy = element["copyText"]?.ToString();
                            bool reload = element["reload"] == null || (bool)element["reload"];
                            bool transparent = element["transparent"] != null && (bool)element["transparent"];
                            bool forcePlayer = element["forcePlayer"] != null && (bool)element["forcePlayer"];
                            float timer = element["timer"] != null ? (float)element["timer"] : 0f;
                            
                            if (transparent)
                                buttonComp.targetGraphic.color = new Color(0, 0, 0, 0);
                            if (forcePlayer)
                                obj.AddComponent<InteractableOnPlayer>();
                            if (timer > 0)
                                Numerators.instance.StartCoroutine(Numerators.instance.TimerButton(buttonComp, timer));

                            if (action == "post") obj.AddComponent<EnterButton>();

                            buttonComp.onClick.AddListener(() =>
                            {
                                switch (action)
                                {
                                    case "load":
                                        LoadWebsite(url, reload);
                                        break;
                                    case "post":
                                        if (!string.IsNullOrEmpty(inputFieldName))
                                        {
                                            var inputField = inputFields.FirstOrDefault(f => f.Item1 == inputFieldName).Item2;
                                            if (inputField != null)
                                            {
                                                PlayerPrefs.SetString("Ultranet_InputField_" + inputFieldName, "");
                                                PostWebsite(url, new Dictionary<string, string> { { "input", inputField.text }, { "token", GetToken() }, { "position", GetPosition() }, { "level", SceneHelper.CurrentScene }, { "version", PluginInfo.Version } }, reload);
                                            }
                                            else
                                            {
                                                Plugin.LogWarning($"Input field with name '{inputFieldName}' not found for button '{obj.name}'.");
                                            }
                                        }
                                        break;
                                    case "postToken":
                                        PostWebsite(url, new Dictionary<string, string> { { "token", GetToken() }, { "level", SceneHelper.CurrentScene }, { "version", PluginInfo.Version } }, reload);
                                        break;
                                    case "open":
                                        Application.OpenURL(url);
                                        break;
                                    case "copy":
                                        GUIUtility.systemCopyBuffer = copy;
                                        break;
                                    default:
                                        Plugin.LogWarning($"Unknown button action: {action}");
                                        break;
                                }
                            });
                        }
                        break;
                    case "inputField":
                        var inputFieldComp = obj.GetComponentInChildren<TMP_InputField>();
                        if (inputFieldComp != null)
                        {
                            inputFieldComp.GetComponent<RectTransform>().sizeDelta = obj.GetComponent<RectTransform>().sizeDelta - Vector2.one * 10;
                            inputFieldComp.gameObject.AddComponent<GayInputField>();
                            inputFieldComp.placeholder.GetComponent<TMP_Text>().text = TextParser.Parse(element["text"]?.ToString()) ?? "Enter text...";
                            inputFields.Add((obj.name, inputFieldComp));
                            if (element["saveValue"] != null)
                            {
                                string savedValue = PlayerPrefs.GetString("Ultranet_InputField_" + obj.name, "");
                                string savedCursorPosStr = PlayerPrefs.GetString("Ultranet_InputField_" + obj.name + "_CursorPos", "0");
                                inputFieldComp.text = savedValue;
                                try
                                {
                                    inputFieldComp.caretPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos");
                                    inputFieldComp.selectionAnchorPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos2");
                                    inputFieldComp.selectionFocusPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos3");
                                }
                                catch
                                {

                                }
                                inputFieldComp.onEndEdit.AddListener((value) =>
                                {
                                    PlayerPrefs.SetString("Ultranet_InputField_" + obj.name, value);
                                    PlayerPrefs.SetInt("Ultranet_InputField_" + obj.name + "_CursorPos", inputFieldComp.stringPosition);
                                    PlayerPrefs.SetInt("Ultranet_InputField_" + obj.name + "_CursorPos2", inputFieldComp.selectionAnchorPosition);
                                    PlayerPrefs.SetInt("Ultranet_InputField_" + obj.name + "_CursorPos3", inputFieldComp.selectionFocusPosition);
                                });
                            }
                            if (element["selectOnLoad"] != null)
                            {
                                inputFieldComp.Select();
                            }
                        }
                        break;
                    case "player":
                        var playerButtonComp = obj.GetComponentInChildren<Button>();
                        var playerButtonTextComp = obj.GetComponentInChildren<TMP_Text>();
                        if (playerButtonComp != null)
                        {
                            playerButtonTextComp.text = TextParser.Parse(element["text"]?.ToString()) ?? "Button";
                            playerButtonTextComp.alignment = element["alignment"] != null ? (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), element["alignment"].ToString()) : TextAlignmentOptions.Center;
                            if (element["color"] != null)
                                playerButtonTextComp.color = ParseColor(element["color"]?.ToString());

                            string url = element["url"]?.ToString();
                            string action = element["action"]?.ToString();

                            playerButtonComp.onClick.AddListener(() =>
                            {
                                switch (action)
                                {
                                    case "postToken":
                                        PostWebsite(url, new Dictionary<string, string> { { "token", GetToken() }, { "level", SceneHelper.CurrentScene }, { "version", PluginInfo.Version } });
                                        break;
                                    default:
                                        Plugin.LogWarning($"Unknown player button action: {action}");
                                        break;
                                }
                            });
                        }
                        var playerImageComp = obj.transform.GetComponentInChildren<ImageGetter>();
                        if (playerImageComp != null)
                        {
                            string imageUrl = element["imageUrl"]?.ToString();
                            playerImageComp.imageUrl = imageUrl;
                            playerImageComp.SetImg();
                        }
                        break;
                }
            }

            if (root["scrollbar"] != null)
            {
                float scrollbar = float.Parse(root["scrollbar"].ToString());
                UnityEngine.Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = scrollbar;
                resetScrollbar.ResetPos(scrollbar);
                UnityEngine.Canvas.ForceUpdateCanvases();
            }

            lastJson = json;
        }

        #region HELPERS
        public Color ParseColor(string colorStr)
        {
            if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
            {
                return color;
            }
            return Color.white;
        }

        public void CleanUp()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
            titleText.text = "LOADING...";
        }

        public static string GetToken()
        {
            return PlayerPrefs.GetString("UltranetToken", "");
        }

        public static string GetPosition()
        {
            return Camera.main.transform.position.ToString();
            return NewMovement.Instance != null && NewMovement.Instance.activated ? NewMovement.Instance.transform.position.ToString() : "";
        }
        #endregion
    }
}