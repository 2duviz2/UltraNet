using TMPro;
using UnityEngine;

namespace UltraNet.Canvas;

public class Tooltip : MonoBehaviour
{
	public static Tooltip instance;

	private RectTransform rectTransform;

	private RectTransform canvasRect;

	public void Awake()
	{
		instance = this;
		rectTransform = GetComponent<RectTransform>();
		canvasRect = GetComponentInParent<UnityEngine.Canvas>().GetComponent<RectTransform>();
		base.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out var localPoint);
		rectTransform.localPosition = localPoint;
		if (Input.GetMouseButtonDown(0))
		{
			HideTooltip();
		}
	}

	public static void ShowTooltip(string tooltip)
	{
		instance.Update();
		instance.gameObject.SetActive(value: true);
		instance.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = tooltip;
	}

	public static void HideTooltip()
	{
		instance.gameObject.SetActive(value: false);
	}
}
