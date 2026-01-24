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

        public float syncTime = 0.3f;

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
            StartCoroutine(Numerators.PostRequest(syncUrl, new Dictionary<string, string> { { "token", ContentManager.GetToken() }, { "position", ContentManager.GetPosition() }, { "level", SceneHelper.CurrentScene }, { "cheats", CheatsActive().ToString() } }, (json) =>
            {
                _busy = false;
                if (json != null)
                {
                    ParseJson(json);
                }
            }));
        }

        public GameObject CreatePlayer(string id, Vector3 pos, string name)
        {
            GameObject plr = /*GameObject.CreatePrimitive(PrimitiveType.Sphere);*/ new GameObject("Player viewer");
            /*Destroy(plr.GetComponent<Collider>());
            Renderer r = plr.GetComponent<MeshRenderer>();
            r.material = new Material(DefaultReferenceManager.Instance.masterShader);*/
            plr.transform.position = pos;
            UltraNet.Classes.Player p = plr.AddComponent<UltraNet.Classes.Player>();
            p.CreateName(name);
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

            foreach (var plr in players)
            {
                if (plr.Value == null)
                    players.Remove(plr.Key);
            }

            List<string> iteratedPlayers = [];
            foreach (var prop in (JObject)root["players"])
            {
                string id = prop.Key;
                JObject player = (JObject)prop.Value;
                string positionString = player["position"]?.ToString();
                string playerName = player["name"]?.ToString();
                string isoTimeStamp = player["timestamp"].ToString();
                string cheatsString = player["cheats"].ToString();
                bool cheats = cheatsString.ToLower() == "true";
                DateTime dateTime = DateTime.Parse(isoTimeStamp, null, System.Globalization.DateTimeStyles.AssumeUniversal);
                Vector3 position = ParseVector3(positionString);
                GameObject foundPlayer = players.FirstOrDefault(p => p.Key == id).Value;

                iteratedPlayers.Add(id);

                if (foundPlayer == null)
                    foundPlayer = CreatePlayer(id, position, playerName);
                foundPlayer.GetComponent<UltraNet.Classes.Player>().SetTarget(position, dateTime, cheats);
            }

            foreach (var plr in players)
            {
                if (!iteratedPlayers.Contains(plr.Key))
                {
                    GameObject p = plr.Value;
                    players.Remove(plr.Key);
                    if (p != null)
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
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
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

        public bool CheatsActive()
        {
            return CheatsController.Instance.cheatsEnabled || MonoSingleton<StatsManager>.Instance.majorUsed;
        }
        #endregion
    }
}
