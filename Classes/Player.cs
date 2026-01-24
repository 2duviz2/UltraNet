using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UltraNet.Classes
{
    public class Player : MonoBehaviour
    {
        public float lerpSpeed = 5f;

        Vector3 targetPos;

        public void SetTarget(Vector3 newPos, DateTime newTime)
        {
            //Plugin.LogInfo($"NewPos: {newPos}, newTime: {newTime.ToLongTimeString()}");
            targetPos = newPos;
        }

        public void CreateName(string text)
        {
            GameObject playerName = Instantiate(BundlesManager.netBundle.LoadAsset<GameObject>("PlayerName"));
            playerName.transform.localScale = new Vector3(-0.5f, 0.5f, 1);
            playerName.transform.SetParent(transform, false);
            playerName.GetComponentInChildren<TMP_Text>().text = text;
        }

        public void Update()
        {
            float distance = Vector3.Distance(transform.position, targetPos);
            float speed = distance * lerpSpeed;
            float t = Time.deltaTime * speed;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, t);
            transform.LookAt(Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            //transform.position = Vector3.Lerp(transform.position, targetPos, t);
        }
    }
}