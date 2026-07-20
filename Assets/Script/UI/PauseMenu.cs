using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public InputManager inputManager;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject darkBackground;
    [SerializeField] private GameObject settingPanel;

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        darkBackground.SetActive(false);
        inputManager.isPause = false;
        Time.timeScale = 1f; 
    }

    public void OpenSetting()
    {
        pausePanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
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
