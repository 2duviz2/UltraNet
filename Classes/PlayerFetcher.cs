using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UltraNet.Canvas;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Classes
{
    public class PlayerFetcher : MonoBehaviour
    {
        string syncUrl = "https://duviz.xyz/ultranet/user/update";
        float timer = 0;

        Dictionary<string, GameObject> players = [];

        public float syncTime = 0.2f;

        public void Update()
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= syncTime)
            {
                if (SceneHelper.CurrentScene == "Main Menu" || NewMovement.Instance == null) return;
                timer = 0;
                Sync();
            }
        }

        bool _busy = false;
        public void Sync()
        {
            if (_busy) { timer = syncTime; return; }
            _busy = true;
            StopAllCoroutines();
            StartCoroutine(ContentManager.PostRequest(syncUrl, new Dictionary<string, string> { { "token", ContentManager.GetToken() }, { "position", ContentManager.GetPosition() }, { "level", SceneHelper.CurrentScene } }, (json) =>
            {
                _busy = false;
                if (json != null)
                {
                    ParseJson(json);
                }
            }));
        }

        public GameObject CreatePlayer(string id, Vector3 pos)
        {
            GameObject plr = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(plr.GetComponent<Collider>());
            Renderer r = plr.GetComponent<MeshRenderer>();
            r.material = new Material(DefaultReferenceManager.Instance.masterShader);
            plr.transform.position = pos;
            plr.AddComponent<UltraNet.Classes.Player>();
            players.Add(id, plr);
            return plr;
        }

        public void ParseJson(string json)
        {
            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Failed to parse scene json: {ex}");
                return;
            }

            List<string> iteratedPlayers = [];
            foreach (var prop in (JObject)root["players"])
            {
                string id = prop.Key;
                JObject player = (JObject)prop.Value;
                string positionString = player["position"]?.ToString();
                string isoTimeStamp = player["timestamp"].ToString();
                DateTime dateTime = DateTime.Parse(isoTimeStamp, null, System.Globalization.DateTimeStyles.AssumeUniversal);
                Vector3 position = ParseVector3(positionString);
                GameObject foundPlayer = players.FirstOrDefault(p => p.Key == id).Value;

                iteratedPlayers.Add(id);

                if (foundPlayer == null)
                    foundPlayer = CreatePlayer(id, position);
                foundPlayer.GetComponent<UltraNet.Classes.Player>().SetTarget(position, dateTime);
            }

            foreach (var plr in players)
            {
                if (!iteratedPlayers.Contains(plr.Key))
                {
                    GameObject p = plr.Value;
                    players.Remove(plr.Key);
                    Destroy(p);
                }
            }
        }

        #region HELPERS
        Vector4 ParseVector4(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 4)
            {
                Plugin.LogError($"Invalid Vector4 format: {input}");
                return Vector4.zero;
            }

            return new Vector4(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        public static Vector3 ParseVector3(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 3)
            {
                Plugin.LogError($"Invalid Vector3 format: {input}");
                return Vector3.zero;
            }

            return new Vector3(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture) + 3,
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        public static Vector2 ParseVector2(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 2)
            {
                Plugin.LogError($"Invalid Vector2 format: {input}");
                return Vector2.zero;
            }

            return new Vector2(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture)
            );
        }
        #endregion
    }
}
