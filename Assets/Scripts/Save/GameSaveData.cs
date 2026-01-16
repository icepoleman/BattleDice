using System.Collections.Generic;
using UnityEngine;

// 總體遊戲進度存檔
[System.Serializable]
public class GameSaveData
{
    public string playerName;
    public int currentMap;        // 當前地圖
    public string currentStage;          // 當前關卡
    public string preparationRoomStage; // 前個整備室關卡
    public string saveTimeString; // 存檔時間字串（用於 JSON 序列化）
    public CharacterSaveData playerData;   // 玩家角色資料
    
    // DateTime 屬性（不序列化，用於程式存取）
    public System.DateTime SaveTime
    {
        get => string.IsNullOrEmpty(saveTimeString) ? System.DateTime.MinValue : System.DateTime.Parse(saveTimeString);
        set => saveTimeString = value.ToString("yyyy/MM/dd HH:mm:ss");
    }
    public List<int> hasSkillIDs = new List<int>();    // 擁有的技能ID
    public int gold;                     // 當前金幣數量
    public int gear;                  // 當前齒輪數量(強化素材)
    
    // 升級等級存儲
    public int powerUpLevel_MaxBlood;
    public int powerUpLevel_DiceCount;
    public int powerUpLevel_KeepDiceCount;
    public int powerUpLevel_MaxRollCount;
    //public SettingsSaveData settings;      // 遊戲設定
    //TODO: 取得的關鍵道具 個角色好感度
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