using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // 필요한 컴포넌트
    private Rigidbody2D myrigid;
    public InputManager input;
    private Animator animator;

    // 이동 관련 변수
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 컴포넌트 초기화
        myrigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 이벤트 구독
        input.OnMove += StartMove;
        input.OnMoveEnd += EndMove;
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
        
        InMove(moveInput);
    }

    private void InMove(Vector2 moveInput)
    {
        animator.SetFloat("RunState", 0.5f);
        targetPosition = myrigid.position + (moveInput.normalized * moveSpeed) * Time.fixedDeltaTime;
    }

    private void EndMove()
    {
        animator.SetFloat("RunState", 0f);
        targetPosition = myrigid.position;
    }

    private void StartAttack()
    {
        animator.SetTrigger("Attack");
    }
} 