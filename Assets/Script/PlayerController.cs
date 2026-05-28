using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Attack
    }

    // 필요한 컴포넌트
    private Rigidbody2D myrigid;
    public InputManager input;
    private Animator animator;
    private PlayerState state;

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

        state = PlayerState.Idle;
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
        if (state != PlayerState.Attack) {
            state = PlayerState.Move;
            InMove(moveInput);
        }
    }

    private void InMove(Vector2 moveInput)
    {
        animator.SetFloat("RunState", 0.5f);
        if(moveInput.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(moveInput.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        targetPosition = myrigid.position + (moveInput.normalized * moveSpeed) * Time.fixedDeltaTime;
    }

    private void EndMove()
    {
        animator.SetFloat("RunState", 0f);
        targetPosition = myrigid.position;
    }

    private void StartAttack()
    {
        state = PlayerState.Attack;
        StartCoroutine(AttckCoroutine());
    }

    IEnumerator AttckCoroutine()
    {
        // 공격 애니메이션 재생
        animator.SetTrigger("Attack");
        // 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        EndAttack();
    }

    private void EndAttack()
    {
        state = PlayerState.Idle;
    }
} 