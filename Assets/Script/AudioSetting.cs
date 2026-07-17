using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{

    private const string MASTER_KEY = "MASTER_VOLUME";
    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Sliders")]
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;

    [Header("Debug")]
    [SerializeField] private float masterCurrent; // 현재 master 볼륨 Inspector에서 확인용
    [SerializeField] private float bgmCurrent; // 현재 bgm 볼륨 Inspector에서 확인용
    [SerializeField] private float sfxCurrent; // 현재 sfx 볼륨 Inspector에서 확인용
    
    private void Start()
    {
        InitSlider(masterSlider, masterText, MASTER_KEY, 1f, SetMaster);
        InitSlider(bgmSlider, bgmText, BGM_KEY, 1f, SetBGM);
        InitSlider(sfxSlider, sfxText, SFX_KEY, 1f, SetSFX);

    }

    void InitSlider(
        Slider slider,
        TMP_Text text,
        string key,
        float defaultValue,
        UnityEngine.Events.UnityAction<float> callback)
    {
        float value = PlayerPrefs.GetFloat(key, defaultValue);
        slider.value = value;
        callback(value);
        slider.onValueChanged.AddListener(callback);
    }

    // Slider에서 연결될 함수
    public void SetMaster(float v)
    {
        AudioListener.volume = v; //실제 볼륨 적용
        masterCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(masterText, v);
        PlayerPrefs.SetFloat(MASTER_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] Master Volume: {v}");
    }

    public void SetBGM(float v)
    {
        bgmCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(bgmText, v);
        PlayerPrefs.SetFloat(BGM_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] BGM Volume: {v}");
    }
    public void SetSFX(float v)
    {
        sfxCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(sfxText, v);
        PlayerPrefs.SetFloat(SFX_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] SFX Volume: {v}");
    }

    void UpdateText(TMP_Text text, float v)
    {
        text.text = Mathf.RoundToInt(v * 100f) + "%";
    }
}