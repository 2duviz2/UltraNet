using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraNet.Canvas
{
    public class NotificationListener : MonoBehaviour
    {
        public static NotificationListener instance;
        public Animator animator;

        public void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Show()
        {
            if (instance == null) { Plugin.LogWarning("No NotificationListener found!"); return; }
            instance.animator.SetTrigger("appear");
        }
    }
}