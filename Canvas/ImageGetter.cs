using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class ImageGetter : MonoBehaviour
    {
        public string imageUrl;
        public RawImage image;
        public bool modifySize = false;

        public void SetImg()
        {
            if (!modifySize)
            {
                RectTransform r = GetComponent<RectTransform>();
                RectTransform r2 = image.GetComponent<RectTransform>();
                RectTransform r3 = image.transform.parent.GetComponent<RectTransform>();
                r2.sizeDelta = r.sizeDelta - Vector2.one * 50;
                r3.sizeDelta = r.sizeDelta - Vector2.one * 50;
            }
            image.color = Color.black;
            StartCoroutine(GetTextureFromURL(imageUrl, tex =>
            {
                if (tex != null)
                {
                    image.texture = tex;
                    image.color = Color.white;
                }
            }));
        }

        public static bool _loaded = true;
        public static List<(string, Texture2D)> cachedPngs = [];
        public static IEnumerator GetTextureFromURL(string url, System.Action<Texture2D> callback)
        {
            //while (!_loaded) yield return null;
            _loaded = false;

            (string, Texture2D) cached = cachedPngs.FirstOrDefault(x => x.Item1 == url);

            if (cached.Item2 != null)
            {
                callback(cached.Item2);
                yield break;
            }

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Plugin.LogError("Failed to load texture: " + uwr.error);
                    callback?.Invoke(null);
                    _loaded = true;
                }
                else
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                    tex.filterMode = FilterMode.Point;
                    callback?.Invoke(tex);
                    _loaded = true;
                    cachedPngs.Add((url, tex));
                }
            }
        }
    }
}
