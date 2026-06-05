namespace UltraNet.Canvas;

using UnityEngine;
using UnityEngine.EventSystems;

public class R_WindowDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform parentWindow;

    private RectTransform canvasRect;
    private Vector2 offset;

    void Start()
    {
        canvasRect = GetComponentInParent<UnityEngine.Canvas>().GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerOnCanvas
        );

        offset = parentWindow.anchoredPosition - pointerOnCanvas;
        parentWindow.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerOnCanvas
        );

        Vector2 newPos = pointerOnCanvas + offset;

        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 windowSize = parentWindow.rect.size;

        float minX = -canvasSize.x * canvasRect.pivot.x + windowSize.x * parentWindow.pivot.x;
        float maxX = canvasSize.x * (1f - canvasRect.pivot.x) - windowSize.x * (1f - parentWindow.pivot.x);

        float minY = -canvasSize.y * canvasRect.pivot.y + windowSize.y * parentWindow.pivot.y;
        float maxY = canvasSize.y * (1f - canvasRect.pivot.y) - windowSize.y * (1f - parentWindow.pivot.y);

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        parentWindow.anchoredPosition = newPos;
    }
}