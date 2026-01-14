
using System.Collections.Generic;
//遊戲資料管理器
public static class GameDataManager
{
    public static bool TestMode = false; // 測試模式開關
    public static string PlayerName = "阿祥";
    public static PlayerData PlayerData { get; set; } = new PlayerData();    // 玩家資料
    public static List<ISkillData> HasSkills { get; set; } = new List<ISkillData>();    // 擁有的技能資料
    public static int CurrentMap { get; set; } = 1;    // 當前地圖
    public static string CurrentStage { get; set; }    // 當前關卡資料
    public static string PreparationRoomStage { get; set; } = "PreparationRoom"; //打輸回到的準備室關卡
    public static int Gold { get; set; } = 0;          // 當前金幣數量
    public static int Gear { get; set; } = 0;    // 當前齒輪數量(強化素材)
    public static EnemyData TmpEnemyData { get; set; } = new EnemyData();
    public static string TmpAvgChapter { get; set; } = "Chapter1";
    public static string CompletedStory { get; set; } = "";//打贏劇情(用於打完怪物後) 使用後清空
    
    // 升級等級存儲
    private static Dictionary<PowerUpType, int> powerUpLevels = new Dictionary<PowerUpType, int>()
    {
        { PowerUpType.MaxBlood, 0 },
        { PowerUpType.DiceCount, 0 },
        { PowerUpType.KeepDiceCount, 0 },
        { PowerUpType.MaxRollCount, 0 }
    };
    
    /// <summary>
    /// 取得升級等級
    /// </summary>
    public static int GetPowerUpLevel(PowerUpType type)
    {
        return powerUpLevels.TryGetValue(type, out int level) ? level : 0;
    }
    
    /// <summary>
    /// 設定升級等級
    /// </summary>
    public static void SetPowerUpLevel(PowerUpType type, int level)
    {
        powerUpLevels[type] = level;
    }
    
    /// <summary>
    /// 取得所有升級等級（用於存檔）
    /// </summary>
    public static Dictionary<PowerUpType, int> GetAllPowerUpLevels()
    {
        return new Dictionary<PowerUpType, int>(powerUpLevels);
    }
    
    /// <summary>
    /// 設定所有升級等級（用於讀檔）
    /// </summary>
    public static void SetAllPowerUpLevels(Dictionary<PowerUpType, int> levels)
    {
        if (levels != null)
        {
            powerUpLevels = new Dictionary<PowerUpType, int>(levels);
        }
    }
    
    /// <summary>
    /// 重置升級等級（新遊戲用）
    /// </summary>
    public static void ResetPowerUpLevels()
    {
        powerUpLevels = new Dictionary<PowerUpType, int>()
        {
            { PowerUpType.MaxBlood, 0 },
            { PowerUpType.DiceCount, 0 },
            { PowerUpType.KeepDiceCount, 0 },
            { PowerUpType.MaxRollCount, 0 }
        };
    }
}