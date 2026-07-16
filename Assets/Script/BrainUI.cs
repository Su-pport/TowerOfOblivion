using UnityEngine;

public class BrainUI : MonoBehaviour
{
    
    public PlayerStatManager stat; //stat의 변수(currentHP, maxHP, currentMP, maxMP)를 가져오기 위해 

    public RectTransform hpFill; //HP_Fill 오브젝트
    public RectTransform mpFill; //MP_Fill 오브젝트

    [Range(0,1)]
    public float hpPercent = 1f; //hp의 퍼센트를 나타내는 변수

    [Range(0,1)]
    public float mpPercent = 1f; //mp의 퍼센트를 나타내는 변수

    private float hpStartHeight; //hp의 처음 시작 높이
    private float mpStartHeight; //mp의 처음 시작 높이

    void Start()
    {
        hpStartHeight = hpFill.sizeDelta.y; //HP_Fill 처음 의 높이를 hpStartHeight에 저장
        mpStartHeight = mpFill.sizeDelta.y; //MP_Fill 처음 의 높이를 mpStartHeight에 저장
    }

    // Update is called once per frame
    void Update()
    {
        float hpPercent = stat.currentHP / stat.maxHP;
        float mpPercent = stat.currentMP / stat.maxMP;

        hpFill.sizeDelta =
            new Vector2(
                hpFill.sizeDelta.x,
                hpStartHeight * hpPercent); //hp가 변할 때 HP_Fill y의 값을 계속 변동

        mpFill.sizeDelta =
            new Vector2(
                mpFill.sizeDelta.x,
                mpStartHeight * mpPercent); // mp가 변할 때 MP_Fill y의 값을 계속 변동
    }
}
