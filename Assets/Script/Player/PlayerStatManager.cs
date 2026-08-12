using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatManager : Stat
{
    // 스텟 포인트
    private int playerLevel = 1;
    [SerializeField] private int LVupGetPoint = 4; // 레벨 업 시 얻는 스텟 포인트
    [HideInInspector] public int statPoint = 0; // 사용하지 않은 스텟 포인트

    //읽기 전용 변수
    public float _moveSpeedRate => moveSpeedRate;
    // ******* 임시
    [SerializeField] private float plusminus = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.init();
    }

    // Update is called once per frame
    void Update()
    {
        RegenerateStamina();

        // 체력, 마나 변경 확인용 임시 함수
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentHP -= plusminus;
            Debug.Log("체력" + plusminus + "감소");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentHP += plusminus;
            Debug.Log("체력" + plusminus + "증가");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentMP -= plusminus;
            Debug.Log("정신력" + plusminus + "감소");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentMP += plusminus;
            Debug.Log("정신력" + plusminus + "증가");

        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            playerLevel += 1;
            Debug.Log("레벨 1증가");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene("EnemyScene");
        }

    }

    public void LevelUp()
    {
        statPoint += LVupGetPoint;
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
