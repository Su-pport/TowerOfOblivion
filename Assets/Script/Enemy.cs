using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject targetUnit;        // 추적할 대상(플레이어)
    public float enemySpeed = 1.0f;      // 몬스터 이동 속도
    public float desiredDistance = 1.0f; // 플레이어와 유지할 최소 거리
    public float detectionRange = 5.0f;  // 플레이어를 감지할 거리

    private Rigidbody2D rb;              // 물리 이동을 위한 Rigidbody2D

    void Start()
    {
        // Rigidbody2D 컴포넌트를 가져옴
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 매 프레임마다 이동 처리 함수 호출
        HandleMovement();
    }

    /// <summary>
    /// 플레이어 추적 및 이동 처리
    /// </summary>
    void HandleMovement()
    {
        // 타겟이 없으면 아무 동작도 하지 않음
        if (targetUnit == null) return;

        // 플레이어 위치 가져오기
        Vector2 targetPos = targetUnit.transform.position;

        // 현재 몬스터와 플레이어 사이의 거리 계산
        float distance = Vector2.Distance(transform.position, targetPos);

        // 플레이어가 감지 범위 안에 들어왔을 때만 추적
        if (distance < detectionRange)
        {
            // 원하는 거리보다 멀리 있을 때만 이동
            if (distance > desiredDistance)
            {
                // Rigidbody2D를 통해 이동 → 충돌 인식 가능
                Vector2 newPos = Vector2.MoveTowards(
                    rb.position,       // 현재 위치
                    targetPos,         // 목표 위치
                    enemySpeed * Time.deltaTime // 이동 속도
                );

                rb.MovePosition(newPos); // 실제 이동 처리
            }
        }
        // 감지 범위 밖이면 대기 (추적하지 않음)
    }
}
