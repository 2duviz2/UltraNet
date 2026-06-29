using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace UltraNet.Classes;

public class SteamAvatarFetcher : MonoBehaviour
{
	private static readonly HttpClient client = new HttpClient();

	public async Task<string> GetSteamAvatarURL(string steamID64)
	{
		string xmlUrl = "https://steamcommunity.com/profiles/" + steamID64 + "/?xml=1";
		try
		{
			string xml = await client.GetStringAsync(xmlUrl);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode avatarFullNode = doc.SelectSingleNode("//avatarFull");
			if (avatarFullNode != null)
			{
				return avatarFullNode.InnerText;
			}
			Plugin.LogWarning("Not found.");
			return null;
		}
		catch (HttpRequestException ex)
		{
			HttpRequestException e = ex;
			Plugin.LogError("HTTP request error: " + e.Message);
			return null;
		}
		catch (XmlException ex2)
		{
			XmlException e2 = ex2;
			Plugin.LogError("XML parsing error: " + e2.Message);
			return null;
		}
	}
}
