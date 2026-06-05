namespace UltraNet.Canvas;

using UnityEngine;

public class R_MainPanel : MonoBehaviour
{
    public GameObject FriendNotification;

    public void Update()
    {
        var friendNoti = false;

        foreach (var notification in NotificationListener.notifications)
        {
            if (notification.type == "friendRequest" || notification.type == "dm")
                friendNoti = true;
        }

        FriendNotification.SetActive(friendNoti);
    }
}
