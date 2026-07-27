using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSetting : MonoBehaviour
{
    public Toggle fullscreenToggle;

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

}
