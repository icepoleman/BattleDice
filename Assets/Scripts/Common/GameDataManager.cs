
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//遊戲資料管理器
public static class GameDataManager
{
    public static string language = "tw";    // 語言設定 (預設為繁體中文)
    public static bool TestMode = false; // 測試模式開關
    public static string PlayerName = "阿祥";
    public static PlayerData PlayerData { get; set; } = new PlayerData();    // 玩家資料
    public static HashSet<int> HasSkillIDs { get; set; } = new HashSet<int>();    // 擁有的技能ID
    public static int CurrentMap { get; set; } = 1;    // 當前地圖

    private static string _currentStage;
    public static string CurrentStage
    {
        get => _currentStage;
        set
        {
            LastCurrentStage = _currentStage;
            _currentStage = value;
        }
    }

    public static string LastCurrentStage { get; private set; }    // 上一次的關卡資料
    public static string PreparationRoomStage { get; set; } = "PreparationRoom"; //打輸回到的準備室關卡
    public static int Gold { get; set; } = 0;          // 當前金幣數量
    public static int Gear { get; set; } = 0;    // 當前齒輪數量(強化素材)
    public static EnemyData TmpEnemyData { get; set; } = new EnemyData();
    public static string TmpAvgChapter { get; set; } = "Chapter1";
    public static string TmpCompletedStory { get; set; } = "";//打贏劇情(用於打完怪物後) 使用後清空
    public static string TmpSaveRoomStory { get; set; } = "";//整備室特殊劇情 
    public static HashSet<string> unlockedAffinityStages { get; set; } = new HashSet<string>();//已解鎖親密度關卡ID
    public static HashSet<string> unlocked_H_Stages { get; set; } = new HashSet<string>();//已解鎖 H 關卡ID
    public static int[] charactersAffinity = { 0, 0, 0, 0 };//角色親密度

    public static bool DreamMode { get; set; } = false;//回看模式

    //提升好感度
    public static async void AddAffinity(string role, int affinityIncrease)
    {
        if (DreamMode)
        {
            Console.WriteLine($"回看模式不增加好感度");
            return; // 回看模式不增加好感度
        }
        // 根據 stageId 決定增加哪個角色的親密度
        switch (role)
        {
            case "JailerGirl":
                charactersAffinity[0] += affinityIncrease;
                break;
            case "Witch":
                charactersAffinity[1] += affinityIncrease;
                break;
            case "WolfGirl":
                charactersAffinity[2] += affinityIncrease;
                break;
            case "Idol":
                charactersAffinity[3] += affinityIncrease;
                break;
            default:
                Console.WriteLine($"未知的角色ID: {role}");
                break;
        }
        await UIManager.ShowHintBubble(LanguageManager.GetFormat("T_AffinityIncrease", affinityIncrease));
    }
    public static int GetRoleAffinity(string role)
    {
        switch (role)
        {
            case "JailerGirl":
                return charactersAffinity[0];
            case "Witch":
                return charactersAffinity[1];
            case "WolfGirl":
                return charactersAffinity[2];
            case "Idol":
                return charactersAffinity[3];
            default:
                Console.WriteLine($"未知的角色ID: {role}");
                return -1;
        }
    }

    //記錄解鎖好感度關卡 H關卡共用
    public static void UnlockAffinityStage(string stageId)
    {
        Console.WriteLine($"嘗試解鎖好感度關卡: {stageId}");
        if (stageId.StartsWith("Affinity_"))
        {
            unlockedAffinityStages.Add(stageId);
            Console.WriteLine($"解鎖好感度關卡: {stageId}");
        }
        if (stageId.StartsWith("H_"))
        {
            unlocked_H_Stages.Add(stageId);
            Console.WriteLine($"解鎖 H 關卡: {stageId}");
        }
    }

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
}