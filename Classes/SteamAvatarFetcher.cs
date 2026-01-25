using UnityEngine;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;

namespace UltraNet.Classes
{
    public class SteamAvatarFetcher : MonoBehaviour
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<string> GetSteamAvatarURL(string steamID64)
        {
            string xmlUrl = $"https://steamcommunity.com/profiles/{steamID64}/?xml=1";

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
                else
                {
                    Plugin.LogWarning("Not found.");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Plugin.LogError("HTTP request error: " + e.Message);
                return null;
            }
            catch (XmlException e)
            {
                Plugin.LogError("XML parsing error: " + e.Message);
                return null;
            }
        }
    }
}
