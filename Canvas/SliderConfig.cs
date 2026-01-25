using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas
{
    public class SliderConfig : MonoBehaviour
    {
        public string pref = "";
        public float defaultValue = 0;

        public Slider slider;

        public void Start()
        {
            slider.value = PlayerPrefs.GetFloat(pref, defaultValue);
        }

        public void Change(int value)
        {
            PlayerPrefs.SetFloat(pref, slider.value);
        }
    }
}
