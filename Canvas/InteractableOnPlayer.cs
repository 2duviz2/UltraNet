using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class InteractableOnPlayer : MonoBehaviour
    {
        Button button;

        public void Start()
        {
            button = GetComponent<Button>();
            button.interactable = false;
        }

        public void Update()
        {
            button.interactable = NewMovement.Instance != null && NewMovement.Instance.activated;
            //button.interactable = NewMovement.Instance != null && SceneHelper.CurrentScene != "Main Menu" && SceneHelper.PendingScene == null;
        }
    }
}
