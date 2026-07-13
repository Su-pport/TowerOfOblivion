using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject MainPanel; //메인 페널
    [SerializeField] private GameObject settingPanel; //세팅 페널

    [Header("Hover Buttons")]
    [SerializeField] private UIButtonHoverEffect[] hoverButtons; //hover 될 수 있는 버튼들의 Text , 여기서 UIHoverEffect Script를 참조해서 가져온거임 그래서 밑 ResetHoverState 함수를 사용할 수 있음

    public void StartGame() //게임 실행
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings() //SettingPanel 열기
    {
        MainPanel.SetActive(false);
        settingPanel.SetActive(true);
        
        ResetAllButtons(); //Buttons의 Hover상태 리셋
    }

    public void CloseSettings() //SettingPanel 닫기
    {
        settingPanel.SetActive(false);
        MainPanel.SetActive(true);

        ResetAllButtons();
    }

    public void ResetAllButtons() //모든 Buttons의 상태 초기화
    {
        foreach (var btn in hoverButtons)
        {
            if (btn != null)
                btn.ResetHoverState();
        }
    }

    public void ExitGame() //게임 끄기
    {
        #if UNITY_EDITOR //유니티에서 실행시켰을시 유니티 재생 종료
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit(); //게임에서 실행시켰을시 게임 종료
        #endif
    }
}
