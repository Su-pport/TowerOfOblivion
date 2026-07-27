using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public StatUI statUI;
    public event Action<Vector2> OnMove; // 이동 입력 이벤트
    public event Action OnMoveEnd; // 이동 종료 이벤트
    public event Action OnAttack; // 공격 입력 이벤트
    public event Action OnRoll; // 구르기 입력 이벤트
    public event Action OnJump; // 점프 입력 이벤트

    [Header("Panel")]
    [SerializeField] private GameObject statPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject darkBackground;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;

    [HideInInspector] public bool isPause = false;

    void Start()
    {
        CloseAllPanel();
        pausePanel.SetActive(false);
        settingPanel.SetActive(false);
    } 
    void Update()
    {
        // 이동 입력 처리
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput != Vector2.zero)
        {
            OnMove?.Invoke(moveInput);
        }
        else
            OnMoveEnd?.Invoke();

        // 공격 입력 처리
        if (Input.GetButtonDown("Fire1"))
        {
            OnAttack?.Invoke();
        }

        // 구르기 입력 처리
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnRoll?.Invoke();
        }

        // 점프 입력 처리 (몬스터 없을 때)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJump?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !isPause)
        {
            if (statPanel.activeSelf)
            {
                CloseAllPanel();
            }
            else
            {
                OpenStat();
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && !isPause)
        {
            if (inventoryPanel.activeSelf)
            {
                CloseAllPanel();
            }
            else
            {
                OpenInventory();
            }            
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause == true)
            {
                pausePanel.SetActive(false);
                settingPanel.SetActive(false);
                darkBackground.SetActive(false);
                isPause = false;
                Time.timeScale = 1f; 
            }
            else
            {
                OpenPause();
            }            
        }
    }

    public void OpenStat()
    {
        CloseAllPanel();

        statPanel.SetActive(true);
        darkBackground.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OpenInventory()
    {
        CloseAllPanel();

        inventoryPanel.SetActive(true);
        darkBackground.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OpenPause()
    {
        CloseAllPanel();

        pausePanel.SetActive(true);
        darkBackground.SetActive (true);

        isPause = true;

        Time.timeScale = 0f;
    }

    public void CloseAllPanel()
    {
        statPanel.SetActive(false);
        statUI.CancelStat();
        inventoryPanel.SetActive(false);

        darkBackground.SetActive(false);

        Time.timeScale = 1f;
    }

}
