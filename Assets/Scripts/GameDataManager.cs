// 統一的資料管理器
public static class GameDataManager
{
    // 玩家資料
    public static PlayerData PlayerData { get; set; } = new PlayerData();
    
    // 當前關卡資料
    public static string CurrentStage { get; set; }
    
    // 當前章節
    public static int CurrentChapter { get; set; } = 1;
    
    // 初始化
    public static void Initialize()
    {
        LoadGameData();
    }
    
    static void LoadGameData(bool loadAutoSave = false)
    {
        // 從存檔載入資料
        if (SaveManager.LoadGame(loadAutoSave))
        {
            PlayerData.LoadFromSaveData(SaveManager.currentSave.playerData);
            CurrentStage = SaveManager.currentSave.currentStage;
            CurrentChapter = SaveManager.currentSave.currentChapter;
        }
    }
}