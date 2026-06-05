namespace UltraNet.Canvas;

using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

public class R_Profile : MonoBehaviour
{
    public TMP_InputField Name;
    public TMP_InputField Bio;
    public TMP_InputField ID;
    public TMP_InputField Pronouns; // Pronouns has been changed to permissions
    public TMP_InputField Status;
    public TMP_InputField SearchField;
    public Image Pfp;
    public GameObject ProfileSettings;
    public GameObject SendFriendRequestButton;

    bool loading = false;

    public void LoadProfile(string steamid)
    {
        if (loading) return;

        id = steamid;
        loading = true;

        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.profileUrl, new() { { "token", ContentManager.GetToken() }, { "steamid", steamid} }, (json) =>
        {
            loading = false;

            var profiles = ProfileParser.Parse(json);

            if (profiles.Count >= 1)
            {
                var p = profiles[0];

                Name.text = $"{p.name} - {p.pronouns}";
                Bio.text = p.description;
                Pronouns.text = p.permissions;
                ID.text = $"#{p.id2}";
                Status.text = p.status;
                LoadPfp(p.id);

                ProfileSettings.SetActive(p.id == ContentManager.steamid);
                SendFriendRequestButton.SetActive(p.id != ContentManager.steamid && p.friends.ToLower() != "true");
            }
            else
            {
                id = "";
                Name.text = "User not found";
                Bio.text = "<size=0>";
                Pronouns.text = "<size=0>";
                ID.text = "<size=0>";
                Status.text = "<size=0>";
                Pfp.sprite = null;
                ProfileSettings.SetActive(false);
                SendFriendRequestButton.SetActive(false);
                return;
            }
        }));
    }

    public void SetAsLoading()
    {
        ProfileSettings.SetActive(false);
        SendFriendRequestButton.SetActive(false);
        Name.text = "Loading...";
        Bio.text = "Loading...";
        Pronouns.text = "Loading...";
        ID.text = "Loading...";
        Status.text = "Loading...";
        Pfp.sprite = null;
    }

    public void Search()
    {
        if (SearchField.text != "")
        {
            SetAsLoading();
            LoadProfile(SearchField.text);
        }
        else
        {
            SetAsLoading();
            LoadProfile(ContentManager.steamid);
        }
    }

    public void SendFriendRequest()
    {
        SendFriendRequestButton.SetActive(false);
        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.sendFriendRequestUrl, new() { { "token", ContentManager.GetToken() }, { "steamid", id } }, (json) => {} ));
    }

    public async void LoadPfp(string steamid)
    {
        if (ulong.TryParse(steamid, out var result))
        {
            var img = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
            Pfp.sprite = img;
        }
    }

    float timer = 0f;
    string id;

    public void Update()
    {
        if (loading) return;
        if (id == "") return;

        timer += Time.unscaledDeltaTime;

        if (timer >= 7)
        {
            timer = 0f;
            LoadProfile(id);
        }
    }
}

[Serializable]
public class Profile
{
    public string description;
    public string messageCount;
    public string permissions;
    public string pronouns;
    public string rank;
    public string name;
    public string id;
    public string id2;
    public string status;
    public string friends;
    public string dms;
}

[Serializable]
public class ProfileList
{
    public List<Profile> profiles;
}

public static class ProfileParser
{
    public static List<Profile> Parse(string json)
    {
        try
        {
            string wrapped = "{ \"profiles\": " + json + "}";
            ProfileList result = JsonUtility.FromJson<ProfileList>(wrapped);
            return result.profiles;
        }
        catch (Exception)
        {
            return new List<Profile>();
        }
        
    }
}

public static class SteamAvatarUtils
{
    public static async Task<Sprite> GetAvatarSpriteAsync(ulong steamId)
    {
        var friend = new Friend(steamId);

        var image = await friend.GetLargeAvatarAsync();
        if (!image.HasValue)
            return null;

        Texture2D tex = ConvertSteamImageToTexture(image.Value);

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public static Texture2D ConvertSteamImageToTexture(Steamworks.Data.Image image)
    {
        Texture2D tex = new Texture2D(
            (int)image.Width,
            (int)image.Height,
            TextureFormat.RGBA32,
            false
        );

        tex.LoadRawTextureData(image.Data);
        tex.Apply();

        return tex;
    }
}