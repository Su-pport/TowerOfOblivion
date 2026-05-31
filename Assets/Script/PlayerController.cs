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
    [SerializeField] private HitEffect hitEffect;

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
    //start, in, end로 나눠서 작성, state는 start에서 변경


    // 이동 시작, 이동 중, 이동 종료 메서드
    private void StartMove(Vector2 moveInput)
    {
        if (state != PlayerState.Attack) // 공격 중이 아닐 때만 이동 시작
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
        targetPosition = myrigid.position + (moveInput.normalized * moveSpeed) * Time.fixedDeltaTime;
    }

    private void EndMove()
    {
        animator.SetFloat("RunState", 0f);
        targetPosition = myrigid.position;
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
} 