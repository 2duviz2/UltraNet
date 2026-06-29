using UnityEngine;

namespace UltraNet.Canvas;

public class R_MainPanel : MonoBehaviour
{
	public GameObject FriendNotification;

	public void Update()
	{
		bool friendNoti = false;
		foreach (Notification notification in NotificationListener.notifications)
		{
			if (notification.type == "friendRequest" || notification.type == "dm")
			{
				friendNoti = true;
			}
		}
		FriendNotification.SetActive(friendNoti);
	}
}
