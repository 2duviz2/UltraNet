using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UltraNet.Canvas;
using UnityEngine;

namespace UltraNet.Classes
{
    public class PlayerFetcher : MonoBehaviour
    {
        public static PlayerFetcher instance;
        public static float syncTime = 0.3f;

        Dictionary<string, GameObject> players = [];
        string syncUrl = "https://duviz.xyz/ultranet/user/update";
        float timer = 0;

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
        int attempts = 0;
        public void Sync()
        {
            attempts++;
            if (attempts >= 10) _busy = false;
            if (_busy) { timer = syncTime; return; }
            _busy = true;
            attempts = 0;
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

        public GameObject CreatePlayer(string id, Vector3 pos, string name, string url)
        {
            GameObject plr = /*GameObject.CreatePrimitive(PrimitiveType.Sphere);*/ new GameObject("Player viewer");
            /*Destroy(plr.GetComponent<Collider>());
            Renderer r = plr.GetComponent<MeshRenderer>();
            r.material = new Material(DefaultReferenceManager.Instance.masterShader);*/
            plr.transform.position = pos;
            UltraNet.Classes.Player p = plr.AddComponent<UltraNet.Classes.Player>();
            p.CreateName(name, id, url);
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

            foreach (var plr in players.ToList())
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
                string playerUrl = player["pfp"]?.ToString();
                string cheatsString = player["cheats"].ToString();
                bool cheats = cheatsString.ToLower() == "true";
                Vector3 position = ParseVector3(positionString);
                GameObject foundPlayer = players.FirstOrDefault(p => p.Key == id).Value;

                iteratedPlayers.Add(id);

                if (foundPlayer == null)
                    foundPlayer = CreatePlayer(id, position, playerName, playerUrl);
                foundPlayer.GetComponent<UltraNet.Classes.Player>().SetTarget(position, cheats);
            }

            foreach (var prop in (JObject)root["events"])
            {
                string id = prop.Key;
                DoEvent(id);
            }

            foreach (var plr in players.ToList())
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

        void DoEvent(string e)
        {
            if (e == "filth")
            {
                GameObject obj = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
                GameObject inst = Instantiate(obj, NewMovement.Instance.transform.position, Quaternion.identity);
            }
            if (e == "stray")
            {
                GameObject obj = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/Projectile Zombie.prefab");
                GameObject inst = Instantiate(obj, NewMovement.Instance.transform.position, Quaternion.identity);
            }
            if (e == "minos")
            {
                GameObject obj = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/MinosPrime.prefab");
                GameObject inst = Instantiate(obj, NewMovement.Instance.transform.position, Quaternion.identity);
            }
            if (e == "explosion")
            {
                GameObject obj = Plugin.Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
                GameObject inst = Instantiate(obj, NewMovement.Instance.transform.position, Quaternion.identity);
            }
            if (e == "gravity")
            {
                Physics.gravity = new Vector3(-Physics.gravity.y, -Physics.gravity.z, -Physics.gravity.x);
            }
            if (e.StartsWith("custom_"))
            {
                GameObject obj = Plugin.Ass<GameObject>($"Assets/Prefabs/{e.Replace("custom_", "")}.prefab");
                GameObject inst = Instantiate(obj, NewMovement.Instance.transform.position, Quaternion.identity);
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
