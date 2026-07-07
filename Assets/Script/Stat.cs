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

    private float attackPower; // 물리 공격력
    private float magicPower; // 마법 공격력
    private float moveSpeedRate; // 이동속도 배율  
    private float shotSpeed; // 연사속도
    private float maxHP; // HP 총량
    private float currentHP; // 현재 HP
    private float maxMP; // MP 총량
    private float currentMP; // 현재 MP
    private float maxST; // 스테미너 총량
    private float currentST; // 현재 스테미너

    //읽기 전용 변수
    public float _moveSpeedRate => moveSpeedRate;

    [Header("--각종 세부 변수--")]
    [Header("스테미너 관련")]

    [Tooltip("스테미너 총량 배율(스탯 * 이 변수)")]
    [SerializeField] float maxSTRate = 10f; // 

    float stRecoveryAmount; // 초당 스테미너 회복량
    [Tooltip("초당 스테미너 회복량 배율(스탯 * 이 변수")]
    [SerializeField] float stRecoveryAmountRate = 2.0f; // 

    [Tooltip("이 시간동안 스테미너의 변동이 없으면 회복 시작")]
    [SerializeField] float checkInterval = 1.5f; // 

    
    float timer = 0f; // 스테미너 변동이 없었던 시간
    float lastValue; // 마지막으로 스테미너가 변동된 값
    bool initialized = false; // 초기화 여부 lastValue가 초기화되지 않았을 때 false, 초기화된 후 true


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 스테미너
        maxST = statStamina * maxSTRate; // 스테미너 총량은 스테미너의 10배로 설정
        currentST = maxST; // 현재 스테미너는 총량으로 초기화
        stRecoveryAmount = statStamina * stRecoveryAmountRate; // 초당 스테미너 회복량 초기화

        // 공격력
    }

    // Update is called once per frame
    void Update()
    {
        SetMoveSpeedRate(); // 지금은 항상 확인하지만 나중에 스탯을 올리는 함수를 짜면 올릴때만 적용하면 됨
        RegenerateStamina();
    }

    // 순발력
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

    // 스테미너
    // 스테미너 회복 함수
    private void RegenerateStamina()
    {
        if (!initialized)
        {
            lastValue = currentST; // 초기화되지 않았으면 현재 스테미너로 초기화
            initialized = true;
            timer = 0f;
        }

        if (currentST < maxST) // 현재 스테미너가 최대 스테미너보다 작을 때만 회복 로직 실행
        {
            if (currentST < lastValue) // 스테미너가 사용되었는지 확인
            {
                timer = 0f; // 변동이 있으면 타이머 초기화
            }
            else
            {
                timer += Time.deltaTime; // 변동이 없으면 타이머 증가
                if (timer >= checkInterval) // 타이머가 체크 간격을 초과하면 회복 시작
                {
                    currentST += stRecoveryAmount * Time.deltaTime; // 스테미너 회복량 계산
                    Debug.Log(currentST + "/" + maxST);
                    if (currentST >= maxST) // 최대 스테미너를 초과하지 않도록 제한
                    {
                        currentST = maxST;
                        Debug.Log(currentST + "/" + maxST);
                        timer = 0f; // 최대 스테미너에 도달하면 타이머 초기화
                    }
                }
            }
        }
    }

    // 사용할 스테미너 양을 입력받고, 사용가능하면 true, 부족하면 false를 반환하는 함수
    public bool UseStamina(float amount)
    {
        if (amount > currentST)
        {
            Debug.Log("스테미너가 부족합니다.");
            return false; // 스테미너가 부족하여 사용할 수 없음
        }
        else
        {
            currentST -= amount;
            Debug.Log(currentST + "/" + maxST);
            initialized = false;
            return true;
        }
    }
}