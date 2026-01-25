using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.WSA;

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


        closeButton.onClick.AddListener(() => Destroy(gameObject));

        textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
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
        AudioManager.Instance.PlaySFX("Sfx_Click");
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
}
