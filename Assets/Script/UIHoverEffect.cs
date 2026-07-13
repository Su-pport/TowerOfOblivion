using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Target")]
    [SerializeField] private Image targetImage; //실제 이미지 : 시작적으로 보이는

    [Header("Effect")]
    [SerializeField] private float scaleMultiplier = 1.1f; //커지는 크기 비율
    [SerializeField] private Color normalColor; //일반적인(처음의) 버튼 색깔
    [SerializeField] private Color hoverColor; //마우스를 올리면 되는 버튼 색깔

    private Vector3 originalScale; //처음 버튼 크기

    private void Awake() // private void Start() 함수보다 먼저 실행되는 함수
    {
        if(targetImage != null)
            originalScale = targetImage.transform.localScale; //초기 크기 저장
    } 

    public void OnPointerEnter(PointerEventData eventData) //마우스를 올렸을 때
    {
        targetImage.transform.localScale = originalScale * scaleMultiplier;
        targetImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData) //마우수가 버튼에서 벗어났을때
    {
        targetImage.transform.localScale = originalScale;
        targetImage.color = normalColor;
    }

    public void ResetHoverState() //Hover되어 있는 상태 초기화, MenuManager에서 사용되는 함수
    {
        if(targetImage != null)
        {
            targetImage.transform.localScale = originalScale;
            targetImage.color = normalColor;
        }
    } 
}