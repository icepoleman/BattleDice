using System.Collections.Generic;
using UnityEngine;

// 總體遊戲進度存檔
[System.Serializable]
public class GameSaveData
{
    public string playerName;
    public int currentMap;        // 當前地圖
    public string currentStage;          // 當前關卡
    public System.DateTime saveTime; // 最後存檔時間
    public CharacterSaveData playerData;   // 玩家角色資料
    public List<ISkillData> hasSkills = new List<ISkillData>();    // 擁有的技能資料
    public int gold;                     // 當前金幣數量
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