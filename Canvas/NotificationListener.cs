using System;
using System.Collections.Generic;
using UltraNet.Classes;
using UnityEngine;

namespace UltraNet.Canvas
{
    public class NotificationListener : MonoBehaviour
    {
        public static NotificationListener instance;
        public Animator animator;

        public static List<Notification> notifications = [];

        bool loading = false;
        float timer;

        public void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            Fetch();
        }

        public static void Show()
        {
            if (instance == null) { Plugin.LogWarning("No NotificationListener found!"); return; }
            instance.animator.SetTrigger("appear");
        }

        public void Update()
        {
            if (loading) return;

            timer += Time.unscaledDeltaTime;

            if (timer > 6)
            {
                timer = 0;
                Fetch();
            }
        }

        public void Fetch()
        {
            if (loading) return;

            loading = true;

            Numerators.instance.StartCoroutine(Numerators.PostRequest(ContentManager.getNotificationsUrl, new() { { "token", ContentManager.GetToken() } }, (json) =>
            {
                loading = false;
                notifications = NotificationParser.Parse(json);

                bool read = true;

                foreach (var notification in notifications)
                {
                    if (!notification.read)
                    {
                        read = false;
                        break;
                    }
                }

                if (!read) Show();
            }));
        }
    }

    [Serializable]
    public class Notification
    {
        public string content;
        public string type;
        public bool read;
    }

    [Serializable]
    public class NotificationList
    {
        public List<Notification> notifications;
    }

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
}