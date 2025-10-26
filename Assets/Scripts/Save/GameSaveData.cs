using System.Collections.Generic;
using UnityEngine;

// 總體遊戲進度存檔
[System.Serializable]
public class GameSaveData
{
    public string playerName;
    public int currentChapter;        // 當前章節
    public string currentStage;          // 當前關卡
    public float totalPlayTime;       // 總遊玩時間（秒）
    public System.DateTime lastSaveTime; // 最後存檔時間
    public CharacterSaveData playerData;   // 玩家角色資料
    //public SettingsSaveData settings;      // 遊戲設定
}
[System.Serializable]
public class SettingsSaveData
{
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public bool fullscreen = true;
}