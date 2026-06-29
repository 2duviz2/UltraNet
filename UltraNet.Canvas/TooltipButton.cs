using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraNet.Canvas;

public class TooltipButton : MonoBehaviour
{
	public string tooltip;

	private void Start()
	{
		EventTrigger eventTrigger = base.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry pointerEnter = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		pointerEnter.callback.AddListener(delegate
		{
			Tooltip.ShowTooltip(tooltip);
		});
		EventTrigger.Entry pointerExit = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit
		};
		pointerExit.callback.AddListener(delegate
		{
			Tooltip.HideTooltip();
		});
		eventTrigger.triggers.Add(pointerEnter);
		eventTrigger.triggers.Add(pointerExit);
	}
}
