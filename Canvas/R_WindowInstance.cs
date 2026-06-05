namespace UltraNet.Canvas;

using UnityEngine;

public class R_WindowInstance : MonoBehaviour
{
    public void Start()
    {
        ContentManager.openWindows.Add(name);
        ContentManager.openWindowsObjs.Add(gameObject);
    }

    public void OnDestroy()
    {
        ContentManager.openWindows.Remove(name);
        ContentManager.openWindowsObjs.Remove(gameObject);
    }

    public void DestroyWindow()
    {
        if (ContentManager.openWindowsObjs.Count == 1)
        {
            Plugin.instance.PressKey();
        }

        Destroy(gameObject);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void OpenSelfProfile()
    {
        ContentManager.instance.OpenProfile(ContentManager.steamid);
    }

    public void OpenWindow(string id)
    {
        ContentManager.instance.SpawnWindow(id);
    }

    public void Logout()
    {
        PlayerPrefs.SetString("UltranetToken", "");
        ContentManager.instance.ClearWindows();
    }
}
