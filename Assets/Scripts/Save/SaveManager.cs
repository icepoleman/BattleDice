using UnityEngine;

public static class SaveManager
{
    private static string saveFilePath => Application.persistentDataPath + "/gamesave.json";
    
    public static GameSaveData currentSave = new GameSaveData();
    
    public static void SaveGame()
    {
        try
        {
            currentSave.lastSaveTime = System.DateTime.Now;
            string json = JsonUtility.ToJson(currentSave, true);
            System.IO.File.WriteAllText(saveFilePath, json);
            Debug.Log("遊戲已存檔到: " + saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("存檔失敗: " + e.Message);
        }
    }
    
    public static bool LoadGame()
    {
        try
        {
            if (System.IO.File.Exists(saveFilePath))
            {
                string json = System.IO.File.ReadAllText(saveFilePath);
                currentSave = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("遊戲讀檔成功");
                return true;
            }
            else
            {
                Debug.Log("找不到存檔檔案，建立新遊戲");
                CreateNewGame();
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("讀檔失敗: " + e.Message);
            return false;
        }
    }
    
    public static void CreateNewGame()
    {
        currentSave = new GameSaveData
        {
            playerName = "玩家",
            currentChapter = 1,
            currentStage = "1-1",
            totalPlayTime = 0f,
            playerData = new PlayerData().ToSaveData(),
            //settings = new SettingsSaveData()
        };
    }
    
    // 更新遊玩時間
    public static void UpdatePlayTime(float deltaTime)
    {
        currentSave.totalPlayTime += deltaTime;
    }
}