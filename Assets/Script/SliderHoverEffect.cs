using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private Image fillImage; 
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;

    private bool isHovered = false;
    private bool isDragging = false;

    private static SliderHoverEffect activeSlider; // 현재 활성화(드래그) 중인 슬라이더 (전역)

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        activeSlider = this;
        UpdateVisual();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (activeSlider == this)
            activeSlider = null;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        bool shouldHighlight =
            isDragging ||
            (isHovered && activeSlider == null) ||
            activeSlider == this;

        fillImage.color = shouldHighlight ? highlightColor : normalColor;
    }
}
