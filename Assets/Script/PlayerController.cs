using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Attack,
        Roll,
        Jump
    }

    // 필요한 컴포넌트
    private Rigidbody2D myRigid;
    public InputManager input; 
    private Animator animator;
    private PlayerState state;
    [SerializeField] private HitEffect hitEffect;
    private Stat stat;

    // 이동 관련 변수
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 targetPosition;

    // 구르기
    private float rollSpeed;
    [SerializeField] private float rollSpeedRate = 1f;
    [SerializeField] private float rollStaminaCost = 5f;
    private Vector2 rollDirection;
    private Vector3 rollTarget;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 컴포넌트 초기화
        myRigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stat = GetComponent<Stat>();
        
        rollSpeed = moveSpeed * rollSpeedRate;

        // 이벤트 구독
        input.OnMove += StartMove;
        input.OnMoveEnd += EndMove;
        input.OnAttack += StartAttack;
        input.OnRoll += StartRoll;
        input.OnJump += StartJump;

        state = PlayerState.Idle;
    }

    // Update is calㅁled once per frame
    void Update()
    {
        
    }

    // 이동 함수
    private void FixedUpdate()
    {
        if(state == PlayerState.Move)
            myRigid.MovePosition(targetPosition);
        if (state == PlayerState.Roll)
        {
            myRigid.MovePosition(myRigid.position + rollDirection * rollSpeed * Time.fixedDeltaTime);
        }
    }

    //start, in, end로 나눠서 작성, state는 start에서 변경

    // 이동 시작, 이동 중, 이동 종료 메서드
    private void StartMove(Vector2 moveInput)
    {
        if (state == PlayerState.Idle || state == PlayerState.Move) // 가만히있거나, 움직일때 계속 움직이게
        { 
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
        targetPosition = myRigid.position + (moveInput.normalized * moveSpeed * stat._moveSpeedRate) * Time.fixedDeltaTime;
    }

    private void EndMove()
    {
        animator.SetFloat("RunState", 0f);
        targetPosition = myRigid.position;
    }

    // 공격 시작, 공격 중, 공격 종료 메서드
    private void StartAttack()
    {
        state = PlayerState.Attack;
        StartCoroutine(AttckCoroutine());
    }

    IEnumerator AttckCoroutine()
    {
        // 공격 애니메이션 재생
        animator.SetTrigger("Attack");
        hitEffect.anim.SetTrigger("Attack");
        // 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        EndAttack();
    }

    private void EndAttack()
    {
        state = PlayerState.Idle;
    }

    // 구르기 시작, 구르기 중, 구르기 종료 메서드
    private void StartRoll()
    {
        if (state == PlayerState.Move) // 움직이고 있을 때만 구르기 가능
        {
            if (stat.UseStamina(rollStaminaCost)) { // 스테미너를 사용, 가능하면 true, 모자르면 false
                StartCoroutine(RollCoroutine());
                state = PlayerState.Roll;
            }
        }
    }

    IEnumerator RollCoroutine()
    {
        // 구르기 애니메이션 재생
        animator.SetTrigger("Roll");

        // 구르기 방향 설정
        rollDirection = (targetPosition - myRigid.position).normalized;


        // 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        EndRoll();
    }

    private void EndRoll()
    {
        state = PlayerState.Idle;
    }

    // 점프 시작, 점프 중, 점프 종료 메서드
    // 기획은 몬스터 없을 때만 점프 가능

    private void StartJump()
    {
        // 몬스터가 없을 때 예외 처리 해야함


    }
}