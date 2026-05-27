using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public event Action<Vector2> OnMove; // 이동 입력 이벤트
    public event Action OnAttack; // 공격 입력 이벤트


    // Update is called once per frame
    void Update()
    {
        // 이동 입력 처리
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (moveInput != Vector2.zero)
        {
            OnMove?.Invoke(moveInput);
        }

        // 공격 입력 처리
        if (Input.GetButtonDown("Fire1"))
        {
            OnAttack?.Invoke();
        }
    }
}
