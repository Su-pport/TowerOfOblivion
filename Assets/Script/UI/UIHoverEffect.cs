using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UIHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Text")]
    [SerializeField] private TMP_Text buttonText; //버튼의 Text

    [Header("Effect")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = Color.black;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private float hoverTextScale = 1.05f;

    private bool isSelected = false; //선택 됐는지 확인 변수

    private Vector3 originalScale; //처음 버튼 크기

    private void Awake() // private void Start() 함수보다 먼저 실행되는 함수
    {
        originalScale = transform.localScale; //초기 크기 저장

        if(buttonText != null)
            buttonText.color = normalTextColor; 
    } 

    //마우스를 올림
    public void OnPointerEnter(PointerEventData eventData) //마우스를 올렸을 때
    {
        //Selected가 아닐 때만 색 변경
        if (!isSelected)
        {
            buttonText.color = hoverTextColor;
        }

        // 글자커지기
        transform.localScale = originalScale * hoverTextScale;   
    }

    //마우스 벗어남
    public void OnPointerExit(PointerEventData eventData) //마우수가 버튼에서 벗어났을때
    {
        if (!isSelected)
        {
            buttonText.color = normalTextColor;
        }
            
        //원래 크기로
        transform.localScale = originalScale;

    }
    
    public void ResetHoverState() //Hover되어 있는 상태 초기화, MenuManager에서 사용되는 함수
    {
        //텍스트 색 원래대로
        buttonText.color = normalTextColor;

        //스케일 원래대로
        transform.localScale = originalScale;
    }


    public void SetSelected(bool selcted)
    {
        isSelected = selcted;

        //선택 상자가 바뀔 때 스케일은 항상 원래로
        buttonText.color = selcted ? selectedTextColor : normalTextColor;
        transform.localScale = originalScale;
    }
}