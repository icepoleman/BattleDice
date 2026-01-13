
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
    public static int GearNum { get; set; } = 0;    // 當前齒輪數量(強化素材)
    public static EnemyData TmpEnemyData { get; set; } = new EnemyData();
    public static string TmpAvgChapter { get; set; } = "Chapter1";
    public static string FightWinStory { get; set; } = "";//打贏劇情(用於打完怪物後) 使用後清空
}