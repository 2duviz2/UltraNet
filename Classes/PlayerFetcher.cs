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

            if (timer > syncTime)
            {
                timer = 0;
                Sync();
            }
        }

        public void Sync()
        {
            StartCoroutine(ContentManager.PostRequest(syncUrl, new Dictionary<string, string> { { "token", ContentManager.GetToken() }, { "position", ContentManager.GetPosition() }, { "level", SceneHelper.CurrentScene } }, (json) =>
            {
                if (json != null)
                {
                    ParseJson(json);
                }
            }));
        }

        public void CreatePlayer(string id, Vector3 pos)
        {
            GameObject plr = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plr.transform.position = pos;
            players.Add(id, plr);
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
            foreach (var player in root["players"])
            {
                iteratedPlayers.Add(player["id"].ToString());


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
    }
}
