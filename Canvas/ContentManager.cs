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

        [Header("Root")]
        public TMP_Text titleText;

        [Header("Prefabs")]
        public GameObject textPrefab;
        public GameObject imagePrefab;
        public GameObject buttonPrefab;
        public GameObject inputFieldPrefab;

        public const string mainUrl = "https://duviz.xyz/static/ultranet/main.pencil";
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
                        'url': 'https://duviz.xyz/static/ultranet/main.pencil',
                        'action': 'load'
                    }
                ]
            }
        ";

        public void Start()
        {
            LoadWebsite(mainUrl);
        }

        public void LoadWebsite(string url, bool deletePrev = true)
        {
            StopAllCoroutines();
            if (deletePrev) CleanUp();
            StartCoroutine(GetStringFromUrl(url, (json) =>
            {
                if (json != null)
                {
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
            if (deletePrev) CleanUp();
            StartCoroutine(PostRequest(url, postData, (json) =>
            {
                if (json != null)
                {
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

            if (root["scrollbar"] != null)
                resetScrollbar.ResetPos((float)root["scrollbar"]);


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

                // Set specific properties
                switch (type)
                {
                    case "text":
                        var textComp = obj.GetComponent<TMP_Text>();
                        if (textComp != null)

                        {
                            textComp.text = element["text"]?.ToString() ?? "Text";
                            textComp.color = ParseColor(element["color"]?.ToString());
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
                            string url = element["url"]?.ToString();
                            string action = element["action"]?.ToString();
                            string inputFieldName = element["inputFieldName"]?.ToString();
                            buttonComp.onClick.AddListener(() =>
                            {
                                switch (action)
                                {
                                    case "load":
                                        LoadWebsite(url);
                                        break;
                                    case "post":
                                        if (!string.IsNullOrEmpty(inputFieldName))
                                        {
                                            var inputField = inputFields.FirstOrDefault(f => f.Item1 == inputFieldName).Item2;
                                            if (inputField != null)
                                            {
                                                PostWebsite(url, new Dictionary<string, string> { { "input", inputField.text }, { "token", GetToken() }});
                                            }
                                            else
                                            {
                                                Plugin.LogWarning($"Input field with name '{inputFieldName}' not found for button '{obj.name}'.");
                                            }
                                        }
                                        break;
                                    case "postToken":
                                        PostWebsite(url, new Dictionary<string, string> { { "token", GetToken() } });
                                        break;
                                    case "open":
                                        Application.OpenURL(url);
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
                        }
                        break;
                }
            }
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
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
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
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
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
        #endregion
    }
}