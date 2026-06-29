using System.Collections.Generic;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Canvas;

public class NotificationListener : MonoBehaviour
{
	public static NotificationListener instance;

	public Animator animator;

	public static List<Notification> notifications = new List<Notification>();

	private bool loading = false;

	private float timer;

	public void Awake()
	{
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Start()
	{
		Fetch();
	}

	public static void Show()
	{
		if (instance == null)
		{
			Plugin.LogWarning("No NotificationListener found!");
		}
		else
		{
			instance.animator.SetTrigger("appear");
		}
	}

	public void Update()
	{
		if (!loading)
		{
			timer += Time.unscaledDeltaTime;
			if (ContentManager.instance.gameObject.activeInHierarchy)
			{
				timer += Time.unscaledDeltaTime * 3f;
			}
			if (timer > 8f)
			{
				timer = 0f;
				Fetch();
			}
		}
	}

	public void Fetch()
	{
		if (loading)
		{
			return;
		}
		loading = true;
		Numerators.instance.StartCoroutine(Numerators.PostRequest("https://duviz.xyz/ultranet/getNotifications", new Dictionary<string, string> { 
		{
			"token",
			ContentManager.GetToken()
		} }, delegate(string json)
		{
			loading = false;
			notifications = NotificationParser.Parse(json);
			if (notifications == null)
			{
				notifications = new List<Notification>();
			}
			bool flag = true;
			foreach (Notification current in notifications)
			{
				if (!current.read)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				Show();
			}
		}));
	}
}
