using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // 필요한 컴포넌트
    private Rigidbody2D myrigid;
    public InputManager input;

    // 이동 관련 변수
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 컴포넌트 초기화
        myrigid = GetComponent<Rigidbody2D>();

        // 이벤트 구독
        input.OnMove += StartMove;
        input.OnAttack += StartAttack;
    }

    // Update is calㅁled once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        myrigid.MovePosition(targetPosition);
    }
    //start, in, end로 나눠서 작성
    private void StartMove(Vector2 moveInput)
    {
        targetPosition = myrigid.position + (moveInput * moveSpeed) * Time.fixedDeltaTime;
    }

    private void StartAttack()
    {
        // 공격 시작 시 처리할 로직
        Debug.Log("공격 시작");
    }
} 