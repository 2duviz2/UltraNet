namespace UltraNet.Canvas;

using TMPro;
using UltraNet.Classes;
using UnityEngine;
using UnityEngine.UI;

public class R_Requests : MonoBehaviour
{
    public TMP_InputField ID;
    public TMP_InputField Field;

    public Transform Container;

    public GameObject RequestItem;

    bool loading = false;
    float timer = 0;

    public void Start()
    {
        ID.text = $"ID: #{ContentManager.compressedid}";

        Fetch();
    }

    public void Update()
    {
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
        Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.getFriendRequestsUrl, new() { { "token", ContentManager.GetToken() } }, (json) =>
        {
            loading = false;
            var profiles = ProfileParser.Parse(json);

            CleanUI();

            foreach (var profile in profiles)
            {
                var item = Instantiate(RequestItem, Container);
                LoadPfp(profile.id, item.transform.GetChild(0).GetChild(0).GetComponent<Image>());
                item.transform.GetChild(1).GetComponent<TMP_InputField>().text = profile.name;

                var profileB = item.transform.GetChild(0).gameObject;
                var accept = item.transform.GetChild(2).gameObject;
                var reject = item.transform.GetChild(3).gameObject;

                profileB.GetComponent<Button>().onClick.AddListener(() =>
                {
                    R_DMs.OpenProfileStatic(profile.id);
                });

                accept.GetComponent<Button>().onClick.AddListener(() =>
                {
                    item.SetActive(false);
                    Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.setFriendsUrl, new() { { "token", ContentManager.GetToken() }, { "steamid", profile.id }, { "status", "true" } }, (response) => { }));
                });

                reject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    item.SetActive(false);
                    Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.setFriendsUrl, new() { { "token", ContentManager.GetToken() }, { "steamid", profile.id }, { "status", "false" } }, (response) => { }));
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

    public async void LoadPfp(string steamid, Image img)
    {
        if (ulong.TryParse(steamid, out var result))
        {
            var i = await SteamAvatarUtils.GetAvatarSpriteAsync(result);
            img.sprite = i;
        }
    }

    public void Search()
    {
        if (string.IsNullOrEmpty(Field.text)) return;
        R_DMs.OpenProfileStatic(Field.text);
    }

    public void CopyID()
    {
        GUIUtility.systemCopyBuffer = ContentManager.compressedid;
    }
}
