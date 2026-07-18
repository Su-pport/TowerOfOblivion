using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{

    private const string MASTER_KEY = "MASTER_VOLUME";
    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";
    private const string UI_KEY = "UI_VOLUME";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    [Header("Text")]
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private TMP_Text uiText;

    [Header("Mute")]
    [SerializeField] private Image masterMuteImage;
    [SerializeField] private Image bgmMuteImage;
    [SerializeField] private Image sfxMuteImage;
    [SerializeField] private Image uiMuteImage;

    [SerializeField] private Sprite muteSprite;
    [SerializeField] private Sprite unmuteSprite;

    private float lastMasterVolume = 1f;
    private float lastBGMVolume = 1f;
    private float lastSFXVolume = 1f;
    private float lastUIVolume = 1f;

    [Header("Debug")]
    [SerializeField] private float masterCurrent; // 현재 master 볼륨 Inspector에서 확인용
    [SerializeField] private float bgmCurrent; // 현재 bgm 볼륨 Inspector에서 확인용
    [SerializeField] private float sfxCurrent; // 현재 sfx 볼륨 Inspector에서 확인용
    [SerializeField] private float uiCurrent; // 현재 ui 볼륨 Inspector에서 확인용
    
    private void Start()
    {
        InitSlider(masterSlider, masterText, MASTER_KEY, 1f, SetMaster);
        InitSlider(bgmSlider, bgmText, BGM_KEY, 1f, SetBGM);
        InitSlider(sfxSlider, sfxText, SFX_KEY, 1f, SetSFX);
        InitSlider(uiSlider, uiText, UI_KEY, 1f, SetUI);

        UpdateAllMuteIcons();
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

        if(v > 0.001f)
        {
            lastMasterVolume = v;
        }

        UpdateAllMuteIcons();
    }

    public void SetBGM(float v)
    {
        bgmCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(bgmText, v);
        PlayerPrefs.SetFloat(BGM_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] BGM Volume: {v}");

        if(v > 0.001f)
        {
            lastBGMVolume = v;
        }

        UpdateAllMuteIcons();
    }
    public void SetSFX(float v)
    {
        sfxCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(sfxText, v);
        PlayerPrefs.SetFloat(SFX_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] SFX Volume: {v}");

        if(v > 0.001f)
        {
            lastSFXVolume = v;
        }

        UpdateAllMuteIcons();
    }

    public void SetUI(float v)
    {
        uiCurrent = v; //Inspector에서 확인하기 위한 값

        UpdateText(uiText, v);
        PlayerPrefs.SetFloat(UI_KEY, v);

        // 볼륨 확인용 로그
        Debug.Log($"[AudioSetting] UI Volume: {v}");

        if(v > 0.001f)
        {
            lastUIVolume = v;
        }

        UpdateAllMuteIcons();
    }

    void UpdateText(TMP_Text text, float v)
    {
        text.text = Mathf.RoundToInt(v * 100f) + "%";
    }

    void UpdateAllMuteIcons()
    {
        masterMuteImage.sprite =
            masterSlider.value <= 0.001f
            ? muteSprite : unmuteSprite;

        bgmMuteImage.sprite =
            bgmSlider.value <= 0.001f
            ? muteSprite : unmuteSprite;

        sfxMuteImage.sprite =
            sfxSlider.value <= 0.001f
            ? muteSprite : unmuteSprite;

        uiMuteImage.sprite =
            uiSlider.value <= 0.001f
            ? muteSprite : unmuteSprite;      
    }

    public void ToggleMasterMute()
    {
        if(masterSlider.value > 0)
        {
            lastMasterVolume = masterSlider.value;
            masterSlider.value = 0;
        }
        else
        {
            masterSlider.value = lastMasterVolume;
        }

        UpdateAllMuteIcons();
    }

    public void ToggleBGMMute()
    {
        if(bgmSlider.value > 0)
        {
            lastBGMVolume = bgmSlider.value;
            bgmSlider.value = 0;
        }
        else
        {
            bgmSlider.value = lastBGMVolume;
        }

        UpdateAllMuteIcons();
    }

    public void ToggleSFXMute()
    {
        if(sfxSlider.value > 0)
        {
            lastSFXVolume = sfxSlider.value;
            sfxSlider.value = 0;
        }
        else
        {
            sfxSlider.value = lastSFXVolume;
        }

        UpdateAllMuteIcons();
    }

    public void ToggleUIMute()
    {
        if(uiSlider.value > 0)
        {
            lastUIVolume = uiSlider.value;
            uiSlider.value = 0;
        }
        else
        {
            uiSlider.value = lastUIVolume;
        }

        UpdateAllMuteIcons();
    }

}