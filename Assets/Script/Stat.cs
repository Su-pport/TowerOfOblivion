using System;
using UnityEngine;

public class Stat : MonoBehaviour
{
    [Header("플레이어가 분배하는 스탯")]
    [SerializeField] public float statAttackPower; // 공격력
    [SerializeField] public float statMagicPower;  // 마력
    [SerializeField] public float statAgility;     // 순발력
    [SerializeField] public float statHealth;      // 체력
    [SerializeField] public float statWillPower;   // 정신력
    [SerializeField] public float statStamina;     // 지구력


    

    [Header("--각종 세부 변수--")]

    [Header("공격력 관련")]
    [Tooltip("물리 공격력 배율(스탯 * 이 변수)")]
    [SerializeField] float attackPowerRate = 5;

    [HideInInspector] public float attackPower; // 물리 공격력

    [Header("마력 관련")]
    [Tooltip("마법 공격력 배율(스탯 * 이 변수)")]
    [SerializeField] float magicPowerRate = 5;

    [HideInInspector] public float magicPower; // 마법 공격력

    [Header("순발력 관련")]
    protected float moveSpeedRate; // 이동속도 배율
                                 // 값 변경은 SetMoveSpeedRate() 참고

    [Header("체력 관련")]
    [Tooltip("체력 총량 배율(스탯 * 이 변수)")]
    [SerializeField] private float maxHPRate = 10;

    [HideInInspector] public float maxHP; // HP 총량
    [HideInInspector] public float currentHP; // 현재 HP

    [Header("정신력 관련")]
    [Tooltip("총량 배율(스탯 * 이 변수)")]
    [SerializeField] private float maxMPRate = 10;

    [HideInInspector] public float maxMP; // MP 총량
    [HideInInspector] public float currentMP; // 현재 MP

    [Header("스테미너 관련")]
    [Tooltip("스테미너 총량 배율(스탯 * 이 변수)")]
    [SerializeField] protected float maxSTRate = 10f; //
    
    [Tooltip("초당 스테미너 회복량 배율(스탯 * 이 변수")]
    [SerializeField] protected float stRecoveryAmountRate = 2.0f; // 

    [Tooltip("이 시간동안 스테미너의 변동이 없으면 회복 시작")]
    [SerializeField] protected float checkInterval = 1.5f; // 

    public float maxST; // 스테미너 총량
    public float currentST; // 현재 스테미너

    protected float stRecoveryAmount; // 초당 스테미너 회복량


    protected float timer = 0f; // 스테미너 변동이 없었던 시간
    protected float lastValue; // 마지막으로 스테미너가 변동된 값
    protected bool initialized = false; // 초기화 여부 lastValue가 초기화되지 않았을 때 false, 초기화된 후 true


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void init()
    {
        // 기본 능력치 초기화
        attackPower = statAttackPower * attackPowerRate;
        magicPower = statMagicPower * magicPowerRate;
        maxHP = statHealth * maxHPRate;
        maxMP = statWillPower * maxMPRate;
        maxST = statStamina * maxSTRate;

        currentHP = maxHP;
        currentMP = maxMP;

        currentST = maxST; // 현재 스테미너는 총량으로 초기화
        stRecoveryAmount = statStamina * stRecoveryAmountRate; // 초당 스테미너 회복량 초기화

        SetMoveSpeedRate();// 지금은 처음에만 확인하지만 나중에 스탯을 올리는 함수를 짜면 올릴때만 적용하면 됨
    }


    // 순발력 속도 조정
    private void SetMoveSpeedRate()
    {
        if (statAgility < 31) // 30 까지는 (_statAgility+100)/100 으로 증가
            moveSpeedRate = 1.0f + statAgility / 100;
        else if (statAgility < 51) // 30~50은 그 전의 절반
        {
            moveSpeedRate = 1.3f + (statAgility - 30) / 200;
        }
        else // 그 이후로는 거의 미비하게
            moveSpeedRate = 1.4f + (statAgility - 50) / 500;

    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;
    }
}