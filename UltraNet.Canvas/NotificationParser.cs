using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraNet.Canvas;

public static class NotificationParser
{
	public static List<Notification> Parse(string json)
	{
		try
		{
			string wrapped = "{ \"notifications\": " + json + "}";
			NotificationList result = JsonUtility.FromJson<NotificationList>(wrapped);
			return result.notifications;
		}
		catch (Exception)
		{
			return new List<Notification>();
		}
	}
}
