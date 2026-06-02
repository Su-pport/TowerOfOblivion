using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverEffect : MonoBehaviour, 
IPointerEnterHandler,
IPointerExitHandler
{
    private Vector3 originalScale;

    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float brightness = 1.2f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleMultiplier;
        canvasGroup.alpha = brightness;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }
}
