using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using UltraNet.Canvas;
using UnityEngine;

namespace UltraNet.Classes;

public class PlayerFetcher : MonoBehaviour
{
	public static PlayerFetcher instance;

	public static float syncTime = 0.4f;

	private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

	private string syncUrl = "https://duviz.xyz/ultranet/user/update";

	private float timer = 0f;

	private bool _busy = false;

	private int attempts = 0;

	public void Update()
	{
		timer += Time.unscaledDeltaTime;
		if (timer >= syncTime && !(MonoSingleton<NewMovement>.Instance == null))
		{
			timer = 0f;
			Sync();
		}
	}

	public void Sync()
	{
		attempts++;
		if (attempts >= 10)
		{
			_busy = false;
		}
		if (_busy)
		{
			timer = syncTime;
			return;
		}
		_busy = true;
		attempts = 0;
		StopAllCoroutines();
		StartCoroutine(Numerators.PostRequest(syncUrl, new Dictionary<string, string>
		{
			{
				"token",
				ContentManager.GetToken()
			},
			{
				"position",
				ContentManager.GetPosition()
			},
			{
				"level",
				SceneHelper.CurrentScene
			},
			{
				"cheats",
				CheatsActive().ToString()
			}
		}, delegate(string json)
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
		GameObject plr = new GameObject("Player viewer");
		plr.transform.position = pos;
		Player p = plr.AddComponent<Player>();
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
		catch (Exception arg)
		{
			Plugin.LogError($"Failed to parse scene json: {arg}");
			return;
		}
		foreach (KeyValuePair<string, GameObject> plr in players.ToList())
		{
			if (plr.Value == null)
			{
				players.Remove(plr.Key);
			}
		}
		List<string> iteratedPlayers = new List<string>();
		foreach (KeyValuePair<string, JToken> prop in (JObject)root["players"])
		{
			string id = prop.Key;
			JObject player = (JObject)prop.Value;
			string positionString = player["position"]?.ToString();
			string playerName = player["name"]?.ToString();
			string playerUrl = player["pfp"]?.ToString();
			string cheatsString = player["cheats"].ToString();
			bool cheats = cheatsString.ToLower() == "true";
			Vector3 position = ParseVector3(positionString);
			GameObject foundPlayer = players.FirstOrDefault((KeyValuePair<string, GameObject> keyValuePair) => keyValuePair.Key == id).Value;
			iteratedPlayers.Add(id);
			if (foundPlayer == null)
			{
				foundPlayer = CreatePlayer(id, position, playerName, playerUrl);
			}
			foundPlayer.GetComponent<Player>().SetTarget(position, cheats);
		}
		foreach (KeyValuePair<string, JToken> item in (JObject)root["events"])
		{
			string id2 = item.Key;
			DoEvent(id2);
		}
		foreach (KeyValuePair<string, GameObject> plr2 in players.ToList())
		{
			if (!iteratedPlayers.Contains(plr2.Key))
			{
				GameObject p = plr2.Value;
				players.Remove(plr2.Key);
				if (p != null)
				{
					UnityEngine.Object.Destroy(p);
				}
			}
		}
	}

	private void DoEvent(string e)
	{
		if (e == "filth")
		{
			GameObject obj = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
			GameObject inst = UnityEngine.Object.Instantiate(obj, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
		if (e == "stray")
		{
			GameObject obj2 = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/Projectile Zombie.prefab");
			GameObject inst2 = UnityEngine.Object.Instantiate(obj2, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
		if (e == "minos")
		{
			GameObject obj3 = Plugin.Ass<GameObject>("Assets/Prefabs/Enemies/MinosPrime.prefab");
			GameObject inst3 = UnityEngine.Object.Instantiate(obj3, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
		if (e == "explosion")
		{
			GameObject obj4 = Plugin.Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
			GameObject inst4 = UnityEngine.Object.Instantiate(obj4, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
		if (e == "gravity")
		{
			Physics.gravity = new Vector3(0f - Physics.gravity.y, 0f - Physics.gravity.z, 0f - Physics.gravity.x);
		}
		if (e.StartsWith("custom_"))
		{
			GameObject obj5 = Plugin.Ass<GameObject>("Assets/Prefabs/" + e.Replace("custom_", "") + ".prefab");
			GameObject inst5 = UnityEngine.Object.Instantiate(obj5, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
	}

	private Vector4 ParseVector4(string input)
	{
		input = input.Trim(new char[3] { '(', ')', ' ' });
		string[] parts = input.Split(',');
		if (parts.Length != 4)
		{
			Plugin.LogError("Invalid Vector4 format: " + input);
			return Vector4.zero;
		}
		return new Vector4(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture));
	}

	public static Vector3 ParseVector3(string input)
	{
		input = input.Trim(new char[3] { '(', ')', ' ' });
		string[] parts = input.Split(',');
		if (parts.Length != 3)
		{
			Plugin.LogError("Invalid Vector3 format: " + input);
			return Vector3.zero;
		}
		return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
	}

	public static Vector2 ParseVector2(string input)
	{
		input = input.Trim(new char[3] { '(', ')', ' ' });
		string[] parts = input.Split(',');
		if (parts.Length != 2)
		{
			Plugin.LogError("Invalid Vector2 format: " + input);
			return Vector2.zero;
		}
		return new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));
	}

	public static bool CheatsActive()
	{
		return MonoSingleton<CheatsController>.Instance.cheatsEnabled || MonoSingleton<StatsManager>.Instance.majorUsed;
	}
}
