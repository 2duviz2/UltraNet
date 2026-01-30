using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UltraNet.Classes
{
    public class Numerators : MonoBehaviour
    {
        public static Numerators instance;
        public static bool _busy = false;

        public void Start()
        {
            instance = this;
        }

        public IEnumerator TimerButton(Button button, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            if (button == null) yield break;
            if (!_busy)
                button.onClick.Invoke();
            StartCoroutine(TimerButton(button, time));
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
            float startTime = Time.realtimeSinceStartup;

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
                    //Plugin.LogInfo($"Time taken: {Time.realtimeSinceStartup - startTime}. Time: {Time.realtimeSinceStartup}");
                    callback?.Invoke(www.downloadHandler.text);
                }
            }
        }
    }
}
