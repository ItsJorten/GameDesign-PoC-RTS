using UnityEngine;
using UnityEngine.UI;

// Beheert het rechthoekje op het scherm voor drag-select.
// We schalen/positioneren deze Image tussen start- en eindpunt van de muis.
[DisallowMultipleComponent]
public class SelectionBoxUI : MonoBehaviour
{
    public Image boxImage; // wijs hier je 'SelectionBox' Image toe
    private RectTransform boxRect;

    private Vector2 startScreenPos;

    void Awake()
    {
        if (boxImage != null) boxRect = boxImage.rectTransform;
        Hide();
    }

    // Aanroepen als we beginnen met slepen
    public void Begin(Vector2 screenPos)
    {
        startScreenPos = screenPos;
        if (boxImage != null)
        {
            boxImage.gameObject.SetActive(true);
            UpdateBox(screenPos);
        }
    }

    // Aanroepen tijdens slepen
    public void Drag(Vector2 currentScreenPos)
    {
        UpdateBox(currentScreenPos);
    }

    // Klaar met slepen
    public void End()
    {
        Hide();
    }

    public bool IsActive => boxImage != null && boxImage.gameObject.activeSelf;

    void Hide()
    {
        if (boxImage != null) boxImage.gameObject.SetActive(false);
    }

    void UpdateBox(Vector2 currentPos)
    {
        if (boxRect == null) return;

        // Bepaal min/max zodat de rect in alle richtingen werkt
        Vector2 min = new Vector2(Mathf.Min(startScreenPos.x, currentPos.x), Mathf.Min(startScreenPos.y, currentPos.y));
        Vector2 max = new Vector2(Mathf.Max(startScreenPos.x, currentPos.x), Mathf.Max(startScreenPos.y, currentPos.y));
        Vector2 size = max - min;

        // Plaats & schaal in schermruimte (Canvas = Screen Space Overlay)
        boxRect.anchoredPosition = min;
        boxRect.sizeDelta = size;
    }

    // Geeft de huidige selectie-rect in pixels terug (min, max)
    public Rect GetScreenRect(Vector2 currentPos)
    {
        Vector2 min = new Vector2(Mathf.Min(startScreenPos.x, currentPos.x), Mathf.Min(startScreenPos.y, currentPos.y));
        Vector2 max = new Vector2(Mathf.Max(startScreenPos.x, currentPos.x), Mathf.Max(startScreenPos.y, currentPos.y));
        return new Rect(min, max - min);
    }
}
