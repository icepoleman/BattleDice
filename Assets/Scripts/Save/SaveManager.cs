using UnityEngine;

public static class SaveManager
{
    private static string saveFilePath => Application.persistentDataPath + "/gamesave.json";
    //整備室自動存檔
    private static string autoSaveFilePath => Application.persistentDataPath + "/autosave.json";
    
    public static GameSaveData currentSave = new GameSaveData();
    
    public static void SaveGame()
    {
        SaveToFile(saveFilePath, "手動存檔");
    }
    
    // 自動存檔方法
    public static void AutoSave()
    {
        SaveToFile(autoSaveFilePath, "自動存檔");
    }
    
    // 統一存檔邏輯
    private static void SaveToFile(string filePath, string saveType)
    {
        try
        {
            currentSave.lastSaveTime = System.DateTime.Now;
            string json = JsonUtility.ToJson(currentSave, true);
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"{saveType}成功: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{saveType}失敗: " + e.Message);
        }
    }
    
    public static bool LoadGame(bool useAutoSave = false)
    {
        string targetFilePath = useAutoSave ? autoSaveFilePath : saveFilePath;
        string saveType = useAutoSave ? "自動存檔" : "手動存檔";
        
        try
        {
            if (System.IO.File.Exists(targetFilePath))
            {
                string json = System.IO.File.ReadAllText(targetFilePath);
                currentSave = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log($"{saveType}讀檔成功");
                return true;
            }
            else
            {
                Debug.Log($"找不到{saveType}檔案，建立新遊戲");
                CreateNewGame();
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{saveType}讀檔失敗: " + e.Message);
            return false;
        }
    }
    
    // 載入自動存檔的便利方法
    public static bool LoadAutoSave()
    {
        return LoadGame(true);
    }
    
    public static void CreateNewGame()
    {
        currentSave = new GameSaveData
        {
            playerName = "玩家",
            currentChapter = 1,
            currentStage = "1-1",
            playerData = new PlayerData().ToSaveData(),
            //settings = new SettingsSaveData()
        };
    }
}