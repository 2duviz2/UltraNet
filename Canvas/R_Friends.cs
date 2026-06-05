namespace UltraNet.Canvas;

using MonoMod.RuntimeDetour.Platforms;
using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

public class R_Friends : MonoBehaviour
{
    public Transform Container;

    public GameObject FriendItem;
    public GameObject RequestsHolder;

    bool loading = false;
    float timer = 0;

    public void Start()
    {
        Fetch();
    }

    public void Update()
    {
        var requests = false;

        foreach (var noti in NotificationListener.notifications)
        {
            if (noti.type == "friendRequest")
            {
                requests = true;
            }
        }

        RequestsHolder.SetActive(requests);

        if (loading) return;

        timer += Time.unscaledDeltaTime;

        if (timer >= 3)
        {
            timer = 0;
            Fetch();
        }
    }

    public void Fetch()
    {
        if (loading) return;
        loading = true;
        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.getFriendsUrl, new() { { "token", ContentManager.GetToken() } }, (json) =>
        {
            loading = false;
            var profiles = ProfileParser.Parse(json);

            CleanUI();

            foreach (var profile in profiles)
            {
                var item = Instantiate(FriendItem, Container);
                LoadPfp(profile.id, item.transform.GetChild(0).GetChild(0).GetComponent<Image>());
                item.transform.GetChild(1).GetComponent<TMP_InputField>().text = profile.name;

                var msg = item.transform.GetChild(2).gameObject;
                var notif = item.transform.GetChild(3).gameObject;

                var hasNoti = false;

                foreach (var noti in NotificationListener.notifications)
                {
                    if (noti.type == "dm" && noti.content == profile.id)
                    {
                        hasNoti = true;
                    }
                }

                notif.SetActive(hasNoti);

                msg.GetComponent<Button>().onClick.AddListener(() =>
                {
                    var window = ContentManager.instance.SpawnWindowCustomID("friendChat", $"friendChat.{profile.dms}");
                    if (window)
                    {
                        var dms = window.GetComponent<R_DMs>();
                        dms.id = profile.id;
                        dms.id2 = profile.id2;
                        dms.dmName = profile.name;
                        dms.chat = profile.dms;
                    }
                });
            }
        }));
    }

    public void CleanUI()
    {
        foreach (Transform child in Container)
        {
            Destroy(child.gameObject);
        }
    }

    public static async void LoadPfp(string steamid, Image img)
    {
        if (ulong.TryParse(steamid, out var result))
        {
            var i = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
            img.sprite = i;
        }
    }
}
