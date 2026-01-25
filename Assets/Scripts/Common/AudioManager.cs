using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 音效管理器 - 統一管理所有音效和背景音樂
/// 
/// 使用範例：
/// // 初始化（在遊戲啟動時呼叫一次）
/// AudioManager.Instance.Initialize();
/// 
/// // 播放單次音效
/// AudioManager.Instance.PlaySFX("SFX/click");
/// AudioManager.Instance.PlaySFX("SFX/explosion", 0.8f);  // 指定音量
/// 
/// // 播放/停止 BGM
/// await AudioManager.Instance.PlayBGM("BGM/battle");
/// AudioManager.Instance.StopBGM();
/// AudioManager.Instance.PauseBGM();
/// AudioManager.Instance.ResumeBGM();
/// 
/// // 控制音量 (0~1)
/// AudioManager.Instance.SetBGMVolume(0.5f);
/// AudioManager.Instance.SetSFXVolume(0.8f);
/// AudioManager.Instance.SetMasterVolume(1.0f);
/// 
/// // 靜音控制
/// AudioManager.Instance.MuteBGM(true);
/// AudioManager.Instance.MuteSFX(true);
/// AudioManager.Instance.MuteAll(true);
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 創建新的 GameObject
                GameObject audioManagerObj = new GameObject("AudioManager");
                _instance = audioManagerObj.AddComponent<AudioManager>();
                DontDestroyOnLoad(audioManagerObj);
            }
            return _instance;
        }
    }
    #endregion

    #region Audio Sources
    private AudioSource _bgmSource;
    private List<AudioSource> _sfxSources = new List<AudioSource>();
    private int _sfxPoolSize = 3;  // SFX 音源池大小
    #endregion

    #region Volume Settings
    private float _masterVolume = 1.0f;
    private float _bgmVolume = 1.0f;
    private float _sfxVolume = 1.0f;

    private bool _isBGMMuted = false;
    private bool _isSFXMuted = false;
    private bool _isMasterMuted = false;
    #endregion

    #region Cache
    // 音效快取 - 避免重複載入
    private Dictionary<string, AudioClip> _audioClipCache = new Dictionary<string, AudioClip>();
    private string _currentBGMAddress = "";
    #endregion

    #region Properties
    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;
    public bool IsBGMMuted => _isBGMMuted;
    public bool IsSFXMuted => _isSFXMuted;
    public bool IsMasterMuted => _isMasterMuted;
    public bool IsBGMPlaying => _bgmSource != null && _bgmSource.isPlaying;
    public string CurrentBGM => _currentBGMAddress;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 初始化音效管理器
    /// </summary>
    public void Initialize()
    {
        Debug.Log("[AudioManager] 初始化音效管理器");

        // 創建 BGM AudioSource
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.priority = 0;  // 最高優先級
        }

        // 創建 SFX AudioSource 池
        CreateSFXPool();

        // 載入保存的音量設定
        LoadVolumeSettings();

        Debug.Log("[AudioManager] 初始化完成");
    }

    private void CreateSFXPool()
    {
        _sfxSources.Clear();
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.priority = 128;
            _sfxSources.Add(sfxSource);
        }
    }
    #endregion

    #region BGM Control
    /// <summary>
    /// 播放背景音樂
    /// </summary>
    /// <param name="address">音樂資源地址</param>
    /// <param name="fadeIn">是否淡入</param>
    /// <param name="fadeDuration">淡入時間</param>
    public async Task PlayBGM(string address, bool fadeIn = false, float fadeDuration = 1.0f)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[AudioManager] BGM 地址為空");
            return;
        }

        // 如果是同一首 BGM，不重複播放
        if (_currentBGMAddress == address && _bgmSource.isPlaying)
        {
            Debug.Log($"[AudioManager] BGM 已在播放中: {address}");
            return;
        }

        AudioClip clip = await LoadAudioClip(address);
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] 無法載入 BGM: {address}");
            return;
        }

        // 停止當前 BGM
        if (_bgmSource.isPlaying)
        {
            _bgmSource.Stop();
        }

        _currentBGMAddress = address;
        _bgmSource.clip = clip;
        _bgmSource.volume = GetActualBGMVolume();

        if (fadeIn)
        {
            _bgmSource.volume = 0;
            _bgmSource.Play();
            await FadeVolume(_bgmSource, 0, GetActualBGMVolume(), fadeDuration);
        }
        else
        {
            _bgmSource.Play();
        }

        Debug.Log($"[AudioManager] 開始播放 BGM: {address}");
    }

    /// <summary>
    /// 停止背景音樂
    /// </summary>
    /// <param name="fadeOut">是否淡出</param>
    /// <param name="fadeDuration">淡出時間</param>
    public async Task StopBGM(bool fadeOut = false, float fadeDuration = 1.0f)
    {
        if (!_bgmSource.isPlaying) return;

        if (fadeOut)
        {
            await FadeVolume(_bgmSource, _bgmSource.volume, 0, fadeDuration);
        }

        _bgmSource.Stop();
        _currentBGMAddress = "";
        Debug.Log("[AudioManager] BGM 已停止");
    }

    /// <summary>
    /// 暫停背景音樂
    /// </summary>
    public void PauseBGM()
    {
        if (_bgmSource.isPlaying)
        {
            _bgmSource.Pause();
            Debug.Log("[AudioManager] BGM 已暫停");
        }
    }

    /// <summary>
    /// 恢復背景音樂
    /// </summary>
    public void ResumeBGM()
    {
        if (!_bgmSource.isPlaying && _bgmSource.clip != null)
        {
            _bgmSource.UnPause();
            Debug.Log("[AudioManager] BGM 已恢復");
        }
    }

    /// <summary>
    /// 切換背景音樂（帶交叉淡入淡出效果）
    /// </summary>
    public async Task CrossFadeBGM(string newAddress, float fadeDuration = 1.0f)
    {
        if (string.IsNullOrEmpty(newAddress)) return;

        AudioClip newClip = await LoadAudioClip(newAddress);
        if (newClip == null) return;

        // 淡出當前 BGM
        if (_bgmSource.isPlaying)
        {
            await FadeVolume(_bgmSource, _bgmSource.volume, 0, fadeDuration / 2);
            _bgmSource.Stop();
        }

        // 淡入新 BGM
        _currentBGMAddress = newAddress;
        _bgmSource.clip = newClip;
        _bgmSource.volume = 0;
        _bgmSource.Play();
        await FadeVolume(_bgmSource, 0, GetActualBGMVolume(), fadeDuration / 2);
    }
    #endregion

    #region SFX Control
    /// <summary>
    /// 播放單次音效
    /// </summary>
    /// <param name="address">音效資源地址</param>
    /// <param name="volumeScale">音量比例 (0~1)</param>
    public async void PlaySFX(string address, float volumeScale = 1.0f)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[AudioManager] SFX 地址為空");
            return;
        }

        AudioClip clip = await LoadAudioClip(address);
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] 無法載入 SFX: {address}");
            return;
        }

        PlaySFXClip(clip, volumeScale);
    }

    /// <summary>
    /// 播放已載入的音效（同步版本，需先預載入）
    /// </summary>
    /// <param name="address">音效資源地址</param>
    /// <param name="volumeScale">音量比例</param>
    /// <returns>是否成功播放</returns>
    public bool PlaySFXImmediate(string address, float volumeScale = 1.0f)
    {
        if (_audioClipCache.TryGetValue(address, out AudioClip clip))
        {
            PlaySFXClip(clip, volumeScale);
            return true;
        }

        Debug.LogWarning($"[AudioManager] SFX 未預載入: {address}，使用 PlaySFX 進行異步載入");
        PlaySFX(address, volumeScale);
        return false;
    }

    private void PlaySFXClip(AudioClip clip, float volumeScale)
    {
        AudioSource availableSource = GetAvailableSFXSource();
        if (availableSource == null)
        {
            Debug.LogWarning("[AudioManager] 沒有可用的 SFX AudioSource");
            return;
        }

        availableSource.clip = clip;
        availableSource.volume = GetActualSFXVolume() * volumeScale;
        availableSource.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 找到沒在播放的 AudioSource
        foreach (var source in _sfxSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 如果都在播放，返回第一個（會打斷它）
        return _sfxSources.Count > 0 ? _sfxSources[0] : null;
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var source in _sfxSources)
        {
            source.Stop();
        }
        Debug.Log("[AudioManager] 所有 SFX 已停止");
    }
    #endregion

    #region Volume Control
    /// <summary>
    /// 設定主音量
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveVolumeSettings();
        Debug.Log($"[AudioManager] 主音量設定為: {_masterVolume}");
    }

    /// <summary>
    /// 設定 BGM 音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        _bgmSource.volume = GetActualBGMVolume();
        SaveVolumeSettings();
        Debug.Log($"[AudioManager] BGM 音量設定為: {_bgmVolume}");
    }

    /// <summary>
    /// 設定 SFX 音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        Debug.Log($"[AudioManager] SFX 音量設定為: {_sfxVolume}");
    }

    /// <summary>
    /// 靜音 BGM
    /// </summary>
    public void MuteBGM(bool mute)
    {
        _isBGMMuted = mute;
        _bgmSource.mute = mute || _isMasterMuted;
        SaveVolumeSettings();
    }

    /// <summary>
    /// 靜音 SFX
    /// </summary>
    public void MuteSFX(bool mute)
    {
        _isSFXMuted = mute;
        foreach (var source in _sfxSources)
        {
            source.mute = mute || _isMasterMuted;
        }
        SaveVolumeSettings();
    }

    /// <summary>
    /// 靜音全部
    /// </summary>
    public void MuteAll(bool mute)
    {
        _isMasterMuted = mute;
        _bgmSource.mute = mute || _isBGMMuted;
        foreach (var source in _sfxSources)
        {
            source.mute = mute || _isSFXMuted;
        }
        SaveVolumeSettings();
    }

    private float GetActualBGMVolume()
    {
        return _masterVolume * _bgmVolume;
    }

    private float GetActualSFXVolume()
    {
        return _masterVolume * _sfxVolume;
    }

    private void UpdateAllVolumes()
    {
        _bgmSource.volume = GetActualBGMVolume();
        // SFX 音量會在播放時設定
    }
    #endregion

    #region Audio Loading
    /// <summary>
    /// 預載入音效
    /// </summary>
    public async Task PreloadAudio(string address)
    {
        await LoadAudioClip(address);
    }

    /// <summary>
    /// 預載入多個音效
    /// </summary>
    public async Task PreloadAudios(params string[] addresses)
    {
        var tasks = new List<Task>();
        foreach (var address in addresses)
        {
            tasks.Add(LoadAudioClip(address));
        }
        await Task.WhenAll(tasks);
    }

    private async Task<AudioClip> LoadAudioClip(string address)
    {
        // 檢查快取
        if (_audioClipCache.TryGetValue(address, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        // 透過 AddressableManager 載入
        AudioClip clip = await AddressableManager.LoadAssetAsync<AudioClip>(address);

        if (clip != null)
        {
            _audioClipCache[address] = clip;
        }

        return clip;
    }

    /// <summary>
    /// 釋放音效快取
    /// </summary>
    public void UnloadAudio(string address)
    {
        if (_audioClipCache.ContainsKey(address))
        {
            _audioClipCache.Remove(address);
            AddressableManager.ReleaseAsset(address);
            Debug.Log($"[AudioManager] 已釋放音效: {address}");
        }
    }

    /// <summary>
    /// 釋放所有音效快取
    /// </summary>
    public void UnloadAllAudio()
    {
        foreach (var address in _audioClipCache.Keys)
        {
            AddressableManager.ReleaseAsset(address);
        }
        _audioClipCache.Clear();
        Debug.Log("[AudioManager] 已釋放所有音效快取");
    }
    #endregion

    #region Settings Persistence
    private const string PREF_MASTER_VOLUME = "AudioManager_MasterVolume";
    private const string PREF_BGM_VOLUME = "AudioManager_BGMVolume";
    private const string PREF_SFX_VOLUME = "AudioManager_SFXVolume";
    private const string PREF_MASTER_MUTED = "AudioManager_MasterMuted";
    private const string PREF_BGM_MUTED = "AudioManager_BGMMuted";
    private const string PREF_SFX_MUTED = "AudioManager_SFXMuted";

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, _masterVolume);
        PlayerPrefs.SetFloat(PREF_BGM_VOLUME, _bgmVolume);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, _sfxVolume);
        PlayerPrefs.SetInt(PREF_MASTER_MUTED, _isMasterMuted ? 1 : 0);
        PlayerPrefs.SetInt(PREF_BGM_MUTED, _isBGMMuted ? 1 : 0);
        PlayerPrefs.SetInt(PREF_SFX_MUTED, _isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOLUME, 0.5f);
        _bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.5f);
        _sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.5f);
        _isMasterMuted = PlayerPrefs.GetInt(PREF_MASTER_MUTED, 0) == 1;
        _isBGMMuted = PlayerPrefs.GetInt(PREF_BGM_MUTED, 0) == 1;
        _isSFXMuted = PlayerPrefs.GetInt(PREF_SFX_MUTED, 0) == 1;

        UpdateAllVolumes();
        MuteAll(_isMasterMuted);
    }
    #endregion

    #region Utility
    private async Task FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            await Task.Yield();
        }
        source.volume = to;
    }

    /// <summary>
    /// 輸出目前狀態
    /// </summary>
    public void LogStatus()
    {
        Debug.Log($"[AudioManager] 狀態報告:");
        Debug.Log($"  主音量: {_masterVolume} (靜音: {_isMasterMuted})");
        Debug.Log($"  BGM 音量: {_bgmVolume} (靜音: {_isBGMMuted})");
        Debug.Log($"  SFX 音量: {_sfxVolume} (靜音: {_isSFXMuted})");
        Debug.Log($"  當前 BGM: {_currentBGMAddress}");
        Debug.Log($"  已快取音效數量: {_audioClipCache.Count}");
    }
    #endregion
}
