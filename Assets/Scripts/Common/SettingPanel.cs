using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SettingPanel : MonoBehaviour
{
    [Header("音量滑桿")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("靜音開關")]
    [SerializeField] private Toggle masterMuteToggle;
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("其他按鈕 (可選)")]
    [SerializeField] private Button closeButton;

    [Header("文字顯示速度")]
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private Text txt_speedShowSample;
    private Coroutine sampleTypingCoroutine;
    private string str_typeSpeedSample;

    [Header("銀幕顯示設定")]
    [SerializeField] private Dropdown dropdown_displayMode;
    private const string SCREEN_MODE_KEY = "ScreenMode";

    [Header("解析度設定")]
    [SerializeField] private Dropdown dropdown_screenSize;
    private const string RESOLUTION_KEY = "Resolution";

    // 16:9 解析度列表
    private readonly (int width, int height)[] resolutions16_9 = new[]
    {
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };

    private void OnEnable()
    {
        InitializeUI();
        RegisterListeners();
    }

    private void OnDestroy()
    {
        UnregisterListeners();
    }

    /// <summary>
    /// 初始化 UI 元件
    /// </summary>
    private void InitializeUI()
    {
        masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
        bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
        sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
        masterMuteToggle.isOn = AudioManager.Instance.IsMasterMuted;
        bgmMuteToggle.isOn = AudioManager.Instance.IsBGMMuted;
        sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted;

        str_typeSpeedSample = LanguageManager.GetText("T_TypeSpeedTest");

        // 文字速度滑桿（0.01~0.1，預設0.05）
        textSpeedSlider.value = ChatWindow.TypingSpeed;
        PlaySampleText(ChatWindow.TypingSpeed);

        // 螢幕模式下拉選單
        InitializeScreenDropdown();

        // 解析度下拉選單
        InitializeResolutionDropdown();
    }

    /// <summary>
    /// 初始化解析度下拉選單
    /// </summary>
    private void InitializeResolutionDropdown()
    {
        dropdown_screenSize.ClearOptions();

        int currentIndex = 0;
        int savedIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, -1);

        for (int i = 0; i < resolutions16_9.Length; i++)
        {
            var res = resolutions16_9[i];
            dropdown_screenSize.options.Add(new Dropdown.OptionData($"{res.width} x {res.height}"));

            // 如果沒有儲存的設定，檢查當前解析度
            if (savedIndex == -1 && Screen.width == res.width && Screen.height == res.height)
            {
                currentIndex = i;
            }
        }

        dropdown_screenSize.value = savedIndex >= 0 ? savedIndex : currentIndex;
        dropdown_screenSize.RefreshShownValue();
    }

    /// <summary>
    /// 初始化螢幕模式下拉選單
    /// </summary>
    private void InitializeScreenDropdown()
    {
        dropdown_displayMode.ClearOptions();
        dropdown_displayMode.options.Add(new Dropdown.OptionData(LanguageManager.GetText("T_Screen_Fullscreen")));
        dropdown_displayMode.options.Add(new Dropdown.OptionData(LanguageManager.GetText("T_Screen_BorderlessWindow")));
        dropdown_displayMode.options.Add(new Dropdown.OptionData(LanguageManager.GetText("T_Screen_Windowed")));

        // 讀取已儲存的螢幕模式
        int savedMode = PlayerPrefs.GetInt(SCREEN_MODE_KEY, 0);
        dropdown_displayMode.value = savedMode;
        dropdown_displayMode.RefreshShownValue();
    }

    /// <summary>
    /// 註冊事件監聽
    /// </summary>
    private void RegisterListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);


        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);


        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        AddPointerUpEvent(sfxVolumeSlider.gameObject, OnSFXSliderPointerUp);

        masterMuteToggle.onValueChanged.AddListener(OnMasterMuteChanged);


        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteChanged);


        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);


        closeButton.onClick.AddListener(() => {
            Destroy(gameObject); 
            EventCenter.Dispatch(StateEvent.EVENT_SETTING_CHANGED); // 通知設定變更
        });

        textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);

        dropdown_displayMode.onValueChanged.AddListener(OnScreenModeChanged);

        dropdown_screenSize.onValueChanged.AddListener(OnResolutionChanged);
    }

    /// <summary>
    /// 取消註冊事件監聽
    /// </summary>
    private void UnregisterListeners()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        masterMuteToggle.onValueChanged.RemoveListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);
        textSpeedSlider.onValueChanged.RemoveListener(OnTextSpeedChanged);
        dropdown_displayMode.onValueChanged.RemoveListener(OnScreenModeChanged);
        dropdown_screenSize.onValueChanged.RemoveListener(OnResolutionChanged);

        if (sampleTypingCoroutine != null)
            StopCoroutine(sampleTypingCoroutine);
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnMasterMuteChanged(bool isOn)
    {
        AudioManager.Instance.MuteAll(isOn);
    }

    private void OnBGMMuteChanged(bool isOn)
    {
        AudioManager.Instance.MuteBGM(isOn);
    }

    private void OnSFXMuteChanged(bool isOn)
    {
        AudioManager.Instance.MuteSFX(isOn);
    }

    #region SFX Slider Test
    /// <summary>
    /// SFX 滑桿拖動完成時播放測試音效
    /// </summary>
    private void OnSFXSliderPointerUp(BaseEventData data)
    {
        AudioManager.Instance.PlaySFX("Sound_Click");
    }

    /// <summary>
    /// 為物件添加 PointerUp 事件
    /// </summary>
    private void AddPointerUpEvent(GameObject target, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }
    #endregion

    #region Text Speed
    /// <summary>
    /// 文字速度變更
    /// </summary>
    private void OnTextSpeedChanged(float value)
    {
        ChatWindow.TypingSpeed = value;
        PlaySampleText(value);
    }

    /// <summary>
    /// 播放範例文字展示效果
    /// </summary>
    private void PlaySampleText(float speed)
    {
        if (sampleTypingCoroutine != null)
            StopCoroutine(sampleTypingCoroutine);
        sampleTypingCoroutine = StartCoroutine(TypeSampleText(speed));
    }

    /// <summary>
    /// 逐字顯示範例文字
    /// </summary>
    private IEnumerator TypeSampleText(float speed)
    {
        txt_speedShowSample.text = "";
        foreach (char c in str_typeSpeedSample)
        {
            txt_speedShowSample.text += c;
            yield return new WaitForSeconds(speed);
        }
        // 顯示完畢後等待一下再重新播放
        yield return new WaitForSeconds(1f);
        PlaySampleText(speed);
    }
    #endregion

    #region Screen Mode
    /// <summary>
    /// 螢幕模式變更
    /// </summary>
    private void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt(SCREEN_MODE_KEY, index);
        PlayerPrefs.Save();

        switch (index)
        {
            case 0: // 全螢幕
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // 視窗全螢幕
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: // 視窗化
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }
    #endregion

    #region Resolution
    /// <summary>
    /// 解析度變更
    /// </summary>
    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutions16_9.Length) return;

        PlayerPrefs.SetInt(RESOLUTION_KEY, index);
        PlayerPrefs.Save();

        var res = resolutions16_9[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }
    #endregion
}
