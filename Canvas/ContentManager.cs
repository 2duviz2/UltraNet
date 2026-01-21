using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
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
        }

        public void OnEnable()
        {
            if (titleText.text.Contains("LOADING"))
            {
                lastJson = "";
                PostWebsite(mainUrl, new Dictionary<string, string> { { "token", GetToken() } });
            }
        }

        public void LoadWebsite(string url, bool deletePrev = true)
        {
            StopAllCoroutines();
            if (deletePrev) { CleanUp(); lastJson = null; }
            StartCoroutine(GetStringFromUrl(url, (json) =>
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
            StopAllCoroutines();
            if (deletePrev) { CleanUp(); lastJson = null; }
            StartCoroutine(PostRequest(url, postData, (json) =>
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

        public void ParseJson(string json)
        {
            if (lastJson == json) return;
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
                            textComp.text = element["text"]?.ToString() ?? "Text";
                            if (element["color"] != null)
                                textComp.color = ParseColor(element["color"]?.ToString());
                            textComp.alignment = element["alignment"] != null ? (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), element["alignment"].ToString()) : TextAlignmentOptions.Center;
                        }
                        break;
                    case "image":
                        var imageComp = obj.transform.GetChild(0).GetComponentInChildren<Image>();
                        if (imageComp != null)
                        {
                            throw new NotImplementedException("Image loading not implemented yet.");
                        }
                        break;
                    case "button":
                        var buttonComp = obj.GetComponent<Button>();
                        var buttonTextComp = obj.GetComponentInChildren<TMP_Text>();
                        if (buttonTextComp != null)
                        {
                            buttonTextComp.text = element["text"]?.ToString() ?? "Button";
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
                                StartCoroutine(TimerButton(buttonComp, timer));

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
                                                PostWebsite(url, new Dictionary<string, string> { { "input", inputField.text }, { "token", GetToken() }, { "position", GetPosition() }, { "level", SceneHelper.CurrentScene } }, reload);
                                            }
                                            else
                                            {
                                                Plugin.LogWarning($"Input field with name '{inputFieldName}' not found for button '{obj.name}'.");
                                            }
                                        }
                                        break;
                                    case "postToken":
                                        PostWebsite(url, new Dictionary<string, string> { { "token", GetToken() } }, reload);
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
                            inputFieldComp.placeholder.GetComponent<TMP_Text>().text = element["text"]?.ToString() ?? "Enter text...";
                            inputFields.Add((obj.name, inputFieldComp));
                            if (element["saveValue"] != null)
                            {
                                string savedValue = PlayerPrefs.GetString("Ultranet_InputField_" + obj.name, "");
                                string savedCursorPosStr = PlayerPrefs.GetString("Ultranet_InputField_" + obj.name + "_CursorPos", "0");
                                inputFieldComp.text = savedValue;
                                inputFieldComp.caretPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos");
                                inputFieldComp.selectionAnchorPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos2");
                                inputFieldComp.selectionFocusPosition = PlayerPrefs.GetInt("Ultranet_InputField_" + obj.name + "_CursorPos3");
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

        public static IEnumerator GetStringFromUrl(string url, System.Action<string> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 5;
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    if (!string.IsNullOrEmpty(www.error))
                        if (www.error.Contains("Unknown Error"))
                            callback?.Invoke("?");
                    Plugin.LogError("Failed to load string: " + www.error);
                    callback?.Invoke(null);
                }
                else
                {
                    callback?.Invoke(www.downloadHandler.text);
                }
            }
        }

        public static IEnumerator PostRequest(string url, Dictionary<string, string> postData, System.Action<string> callback)
        {
            WWWForm form = new WWWForm();
            foreach (var pair in postData)
            {
                form.AddField(pair.Key, pair.Value);
            }
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.timeout = 5;
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    if (!string.IsNullOrEmpty(www.error))
                        if (www.error.Contains("Unknown Error"))
                            callback?.Invoke("?");
                    Plugin.LogError("Failed to post request: " + www.error);
                    callback?.Invoke(null);
                }
                else
                {
                    callback?.Invoke(www.downloadHandler.text);
                }
            }
        }

        public string GetToken()
        {
            return PlayerPrefs.GetString("UltranetToken", "");
        }

        public string GetPosition()
        {
            return NewMovement.Instance != null && NewMovement.Instance.activated ? NewMovement.Instance.transform.position.ToString() : "";
        }

        public IEnumerator TimerButton(Button button, float time)
        {
            if (button == null) yield break;
            yield return new WaitForSecondsRealtime(time);
            button.onClick.Invoke();
            StartCoroutine(TimerButton(button, time));
        }
        #endregion
    }
}