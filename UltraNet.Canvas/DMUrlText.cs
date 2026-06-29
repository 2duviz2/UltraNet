using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraNet.Canvas;

public class DMUrlText : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public TMP_Text text;

	private bool hoveringLink;

	private bool hovering;

	public void OnPointerClick(PointerEventData eventData)
	{
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
		if (linkIndex != -1)
		{
			TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
			string url = linkInfo.GetLinkID();
			Application.OpenURL(url);
		}
	}

	public void Update()
	{
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
		if (linkIndex != -1 && hovering)
		{
			if (!hoveringLink)
			{
				hoveringLink = true;
				Tooltip.ShowTooltip("Open URL");
			}
		}
		else if (hoveringLink)
		{
			hoveringLink = false;
			Tooltip.HideTooltip();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}
}
