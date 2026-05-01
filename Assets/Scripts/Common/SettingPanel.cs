using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class SettingPanel : MonoBehaviour
{
    [Header("音量滑桿")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private TextMeshProUGUI txt_masterVolume;
    [SerializeField] private TextMeshProUGUI txt_bgmVolume;
    [SerializeField] private TextMeshProUGUI txt_sfxVolume;
    [SerializeField] private TextMeshProUGUI txt_masterVolumeTitle;
    [SerializeField] private TextMeshProUGUI txt_bgmVolumeTitle;
    [SerializeField] private TextMeshProUGUI txt_sfxVolumeTitle;

    /*[Header("靜音開關")]
    [SerializeField] private Toggle masterMuteToggle;
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;*/

    [Header("其他按鈕 (可選)")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button btn_close_bg;
    [SerializeField] private Button btn_backToMenu;
    [SerializeField] private Button btn_exit;

    [Header("文字顯示速度")]
    [SerializeField] private TMP_Dropdown dropdown_textSpeed;
    [SerializeField] private TextMeshProUGUI txt_textSpeedTitle;
    private const string TEXT_SPEED_KEY = "TextSpeed";
    private readonly float[] textSpeedValues = { 0.1f, 0.05f, 0.01f };

    [Header("銀幕顯示設定")]
    [SerializeField] private TMP_Dropdown dropdown_displayMode;
    [SerializeField] private TextMeshProUGUI txt_displayModeTitle;
    private const string SCREEN_MODE_KEY = "ScreenMode";

    [Header("解析度設定")]
    [SerializeField] private TMP_Dropdown dropdown_screenSize;
    [SerializeField] private TextMeshProUGUI txt_screenSizeTitle;

    [SerializeField] private TMP_Text txt_title;
    private const string RESOLUTION_KEY = "Resolution";

    // 16:9 解析度列表
    private readonly (int width, int height)[] resolutions16_9 = new[]
    {
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440)
    };

    private void OnEnable()
    {
        txt_title.text = LanguageManager.GetText("T_Setting");
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
        txt_textSpeedTitle.text = LanguageManager.GetText("T_Setting_TextSpeed");
        btn_backToMenu.GetComponent<TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_backMenu");
        btn_exit.GetComponent<TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_ExitGame");
        btn_backToMenu.onClick.AddListener(() =>
        {
            Destroy(gameObject);
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });
        btn_exit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
        bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
        sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
        txt_masterVolume.text = Mathf.RoundToInt(AudioManager.Instance.MasterVolume * 100).ToString();
        txt_bgmVolume.text = Mathf.RoundToInt(AudioManager.Instance.BGMVolume * 100).ToString();
        txt_sfxVolume.text = Mathf.RoundToInt(AudioManager.Instance.SFXVolume * 100).ToString();

        txt_masterVolumeTitle.text = LanguageManager.GetText("T_Master_Volume");
        txt_bgmVolumeTitle.text = LanguageManager.GetText("T_BGM_Volume");
        txt_sfxVolumeTitle.text = LanguageManager.GetText("T_SFX_Volume");
        /*masterMuteToggle.isOn = AudioManager.Instance.IsMasterMuted;
        bgmMuteToggle.isOn = AudioManager.Instance.IsBGMMuted;
        sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted;*/

        // 文字速度下拉選單（慢、正常、快）
        InitializeTextSpeedDropdown();

        // 螢幕模式下拉選單
        InitializeScreenDropdown();

        // 解析度下拉選單
        InitializeResolutionDropdown();
    }

    /// <summary>
    /// 初始化文字速度下拉選單
    /// </summary>
    private void InitializeTextSpeedDropdown()
    {
        dropdown_textSpeed.ClearOptions();
        dropdown_textSpeed.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_TextSpeed_Slow")));
        dropdown_textSpeed.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_TextSpeed_Normal")));
        dropdown_textSpeed.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_TextSpeed_Fast")));

        int defaultIndex = GetClosestTextSpeedIndex(ChatWindow.TypingSpeed);
        int savedIndex = PlayerPrefs.GetInt(TEXT_SPEED_KEY, defaultIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, textSpeedValues.Length - 1);

        dropdown_textSpeed.value = savedIndex;
        dropdown_textSpeed.RefreshShownValue();
        ChatWindow.TypingSpeed = textSpeedValues[savedIndex];
    }

    private int GetClosestTextSpeedIndex(float value)
    {
        int index = 0;
        float minDiff = Mathf.Abs(value - textSpeedValues[0]);
        for (int i = 1; i < textSpeedValues.Length; i++)
        {
            float diff = Mathf.Abs(value - textSpeedValues[i]);
            if (diff < minDiff)
            {
                minDiff = diff;
                index = i;
            }
        }
        return index;
    }

    /// <summary>
    /// 初始化解析度下拉選單
    /// </summary>
    private void InitializeResolutionDropdown()
    {
        dropdown_screenSize.ClearOptions();

        int currentIndex = 0;
        int savedIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, -1);
        savedIndex = Mathf.Clamp(savedIndex, -1, resolutions16_9.Length - 1);

        for (int i = 0; i < resolutions16_9.Length; i++)
        {
            var res = resolutions16_9[i];
            dropdown_screenSize.options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));

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
        dropdown_displayMode.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_Screen_Fullscreen")));
        dropdown_displayMode.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_Screen_BorderlessWindow")));
        dropdown_displayMode.options.Add(new TMP_Dropdown.OptionData(LanguageManager.GetText("T_Screen_Windowed")));

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

        /*masterMuteToggle.onValueChanged.AddListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);*/
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
            EventCenter.Dispatch(StateEvent.EVENT_SETTING_CHANGED); // 通知設定變更
        });
        btn_close_bg.onClick.AddListener(() =>
        {
            Destroy(gameObject);
            EventCenter.Dispatch(StateEvent.EVENT_SETTING_CHANGED); // 通知設定變更
        });

        dropdown_textSpeed.onValueChanged.AddListener(OnTextSpeedChanged);

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
        /*masterMuteToggle.onValueChanged.RemoveListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);*/
        dropdown_textSpeed.onValueChanged.RemoveListener(OnTextSpeedChanged);
        dropdown_displayMode.onValueChanged.RemoveListener(OnScreenModeChanged);
        dropdown_screenSize.onValueChanged.RemoveListener(OnResolutionChanged);
    }

    private void OnMasterVolumeChanged(float value)
    {
        txt_masterVolume.text = Mathf.RoundToInt(value * 100).ToString();
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        txt_bgmVolume.text = Mathf.RoundToInt(value * 100).ToString();
        AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        txt_sfxVolume.text = Mathf.RoundToInt(value * 100).ToString();
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
    private void OnTextSpeedChanged(int index)
    {
        if (index < 0 || index >= textSpeedValues.Length) return;

        ChatWindow.TypingSpeed = textSpeedValues[index];
        PlayerPrefs.SetInt(TEXT_SPEED_KEY, index);
        PlayerPrefs.Save();
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
