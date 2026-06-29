using UnityEngine;

namespace UltraNet.Canvas;

public class R_WindowInstance : MonoBehaviour
{
	public void Start()
	{
		ContentManager.openWindows.Add(base.name);
		ContentManager.openWindowsObjs.Add(base.gameObject);
	}

	public void OnDestroy()
	{
		ContentManager.openWindows.Remove(base.name);
		ContentManager.openWindowsObjs.Remove(base.gameObject);
	}

	public void DestroyWindow()
	{
		if (ContentManager.openWindowsObjs.Count == 1)
		{
			Plugin.instance.PressKey();
		}
		Object.Destroy(base.gameObject);
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
