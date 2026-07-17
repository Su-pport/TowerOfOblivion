using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject MainPanel; //메인 페널
    [SerializeField] private GameObject settingPanel; //세팅 페널
    [SerializeField] private SettingManager settingManager;
    public UIHoverEffect uIHoverEffect; 

    public void StartGame() //게임 실행
    {
        SceneManager.LoadScene("Making_UI");
    }

    public void OpenSettings() //SettingPanel 열기
    {
        MainPanel.SetActive(false);
        settingPanel.SetActive(true);
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
