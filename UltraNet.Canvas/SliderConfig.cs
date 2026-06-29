using UnityEngine;
using UnityEngine.UI;

namespace UltraNet.Canvas;

public class SliderConfig : MonoBehaviour
{
	public string pref = "";

	public float defaultValue = 0f;

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
