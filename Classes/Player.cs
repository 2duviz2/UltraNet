using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraNet.Classes
{
    public class Player : MonoBehaviour
    {
        public float lerpSpeed = 1f;

        Vector3 targetPos;

        public void SetTarget(Vector3 newPos, DateTime newTime)
        {
            Plugin.LogInfo($"NewPos: {newPos}, newTime: {newTime.ToLongTimeString()}");
            targetPos = newPos;
        }

        public void Update()
        {
            float distance = Vector3.Distance(transform.position, targetPos);
            float speed = distance * lerpSpeed;
            float t = Time.deltaTime * speed;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, t);
            //transform.position = Vector3.Lerp(transform.position, targetPos, t);
        }
    }
}