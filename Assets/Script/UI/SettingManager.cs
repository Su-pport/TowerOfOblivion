using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject conveniencePanel;

    [Header("Tab")]
    [SerializeField] private UIHoverEffect audioTab;
    [SerializeField] private UIHoverEffect graphicsTab;
    [SerializeField] private UIHoverEffect controlsTab;
    [SerializeField] private UIHoverEffect convenienceTab;

    private void Start()
    {
        DisableAllPanels();
        ResetTabs();

        ShowAudio();
    }

    public void ShowAudio()
    {
        DisableAllPanels();
        ResetTabs();

        audioPanel.SetActive(true);
        audioTab.SetSelected(true);
    }

    public void ShowGraphics()
    {
        DisableAllPanels();
        ResetTabs();

        graphicsPanel.SetActive(true);
        graphicsTab.SetSelected(true);
    }

    public void ShowControls()
    {
        DisableAllPanels();
        ResetTabs();

        controlsPanel.SetActive(true);
        controlsTab.SetSelected(true);
    }

    public void ShowConvenience()
    {
        DisableAllPanels();
        ResetTabs();

        conveniencePanel.SetActive(true);
        convenienceTab.SetSelected(true);
    }

    private void DisableAllPanels()
    {
        audioPanel.SetActive(false);
        graphicsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        conveniencePanel.SetActive(false);
    }

    private void ResetTabs()
    {
        audioTab.SetSelected(false);
        graphicsTab.SetSelected(false);
        controlsTab.SetSelected(false);
        convenienceTab.SetSelected(false);
    }

    public void CloseSetting()
    {
        ResetToDefaultTab();
        settingPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ResetToDefaultTab()
    {
        DisableAllPanels();
        ResetTabs();

        audioPanel.SetActive(true);
        audioTab.SetSelected(true);
    }


    
}
