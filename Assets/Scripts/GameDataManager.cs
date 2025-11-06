// 統一的資料管理器
public static class GameDataManager
{
    public static string PlayerName = "阿祥";
    // 玩家資料
    public static PlayerData PlayerData { get; set; } = new PlayerData();
    // 當前關卡資料
    public static string CurrentStage { get; set; }
    // 當前地圖
    public static int CurrentMap { get; set; } = 1;

    public static EnemyData TmpEnemyData { get; set; } = new EnemyData();

    public static string AvgChapter { get; set; } = "Chapter1";
    
    // 初始化
    public static void Initialize()
    {
        LoadGameData();
    }

    public static void LoadGameData(bool loadAutoSave = false)
    {
        // 從存檔載入資料
        if (SaveManager.LoadGame(loadAutoSave))
        {
            PlayerName = SaveManager.currentSave.playerName;
            PlayerData.LoadFromSaveData(SaveManager.currentSave.playerData);
            CurrentStage = SaveManager.currentSave.currentStage;
            CurrentMap = SaveManager.currentSave.currentMap;
        }
    }
    public static void SaveGameData(bool saveAutoSave = false)
    {
        if (saveAutoSave)
            SaveManager.AutoSave();
        else
            SaveManager.SaveGame();
    }
}