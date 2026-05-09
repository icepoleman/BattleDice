using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 存档槽信息（用于 UI 显示）
/// </summary>
public class SaveSlotInfo
{
    public int slotIndex;           // 槽位索引
    public bool isEmpty;            // 是否为空槽位
    public string playerName;       // 玩家名称
    public string currentStage;     // 当前关卡
    public string preparationRoomStage; // 整备室关卡
    public int currentMap;          // 当前地图
    public System.DateTime saveTime; // 存档时间
    public int gold;                // 金币数量
    public int gear;             // 齿轮数量
}
public static class SaveManager
{
    // 存档槽数量
    public const int MAX_SAVE_SLOTS = 6;

    // 自动存档路径
    private static string autoSaveFilePath => Application.persistentDataPath + "/autosave.json";

    // 获取指定槽位的存档路径
    private static string GetSlotFilePath(int slotIndex)
    {
        return Application.persistentDataPath + $"/save_{slotIndex}.json";
    }

    public static GameSaveData currentSave = new GameSaveData();

    // ==================== 手动存档接口 ====================

    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    /// <param name="slotIndex">槽位索引 (0-5)</param>
    /// <returns>是否保存成功</returns>
    public static bool SaveToSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;
        return SaveToFile(GetSlotFilePath(slotIndex), $"存檔槽 {slotIndex + 1}");
    }

    /// <summary>
    /// 从指定槽位读取游戏
    /// </summary>
    /// <param name="slotIndex">槽位索引 (0-5)</param>
    /// <returns>是否读取成功</returns>
    public static bool LoadFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;
        return LoadFromFile(GetSlotFilePath(slotIndex), $"存檔槽 {slotIndex + 1}");
    }

    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    /// <param name="slotIndex">槽位索引 (0-5)</param>
    /// <returns>是否删除成功</returns>
    public static bool DeleteSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;

        string filePath = GetSlotFilePath(slotIndex);
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                Debug.Log($"存檔槽 {slotIndex + 1} 刪除成功");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"存檔槽 {slotIndex + 1} 刪除失敗: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 获取指定槽位的存档信息（不载入游戏）
    /// </summary>
    /// <param name="slotIndex">槽位索引 (0-5)</param>
    /// <returns>存档信息，如果没有存档则返回 null</returns>
    public static SaveSlotInfo GetSlotInfo(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return null;

        string filePath = GetSlotFilePath(slotIndex);
        if (!System.IO.File.Exists(filePath)) return null;

        try
        {
            string json = System.IO.File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            return new SaveSlotInfo
            {
                slotIndex = slotIndex,
                playerName = saveData.playerName,
                currentStage = saveData.currentStage,
                preparationRoomStage = saveData.preparationRoomStage,
                currentMap = saveData.currentMap,
                saveTime = saveData.SaveTime,
                gold = saveData.gold,
                gear = saveData.gear,
                isEmpty = false
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"讀取存檔槽 {slotIndex + 1} 資訊失敗: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// 获取所有槽位的存档信息
    /// </summary>
    /// <returns>所有槽位的存档信息数组</returns>
    public static SaveSlotInfo[] GetAllSlotInfos()
    {
        SaveSlotInfo[] infos = new SaveSlotInfo[MAX_SAVE_SLOTS];

        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            infos[i] = GetSlotInfo(i);
            if (infos[i] == null)
            {
                // 空槽位
                infos[i] = new SaveSlotInfo
                {
                    slotIndex = i,
                    isEmpty = true
                };
            }
        }

        return infos;
    }

    // 验证槽位索引
    private static bool IsValidSlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"無效的存檔槽索引: {slotIndex}，有效範圍: 0-{MAX_SAVE_SLOTS - 1}");
            return false;
        }
        return true;
    }

    // ==================== 自动存档 ====================

    /// <summary>
    /// 自动存档
    /// </summary>
    public static void AutoSave()
    {
        SaveToFile(autoSaveFilePath, "自動存檔");
    }

    /// <summary>
    /// 读取自动存档
    /// </summary>
    public static bool LoadAutoSave()
    {
        return LoadFromFile(autoSaveFilePath, "自動存檔");
    }

    /// <summary>
    /// 检查是否存在自动存档
    /// </summary>
    public static bool HasAutoSave()
    {
        return System.IO.File.Exists(autoSaveFilePath);
    }

    /// <summary>
    /// 獲取自動存檔資訊
    /// </summary>
    /// <returns>自動存檔資訊，如果沒有則返回 null</returns>
    public static SaveSlotInfo GetAutoSaveInfo()
    {
        if (!HasAutoSave()) return null;

        try
        {
            string json = System.IO.File.ReadAllText(autoSaveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            return new SaveSlotInfo
            {
                slotIndex = -1, // -1 代表自動存檔
                playerName = saveData.playerName,
                currentStage = saveData.currentStage,
                preparationRoomStage = saveData.preparationRoomStage,
                currentMap = saveData.currentMap,
                saveTime = saveData.SaveTime,
                gold = saveData.gold,
                gear = saveData.gear,
                isEmpty = false
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"讀取自動存檔資訊失敗: " + e.Message);
            return null;
        }
    }

    // ==================== 核心存读档逻辑 ====================

    // 统一存档逻辑
    private static bool SaveToFile(string filePath, string saveType)
    {
        // 将资料存到存档
        currentSave.playerName = GameDataManager.PlayerName;
        currentSave.playerData = GameDataManager.PlayerData.ToSaveData();
        currentSave.currentStage = GameDataManager.CurrentStage;
        currentSave.currentMap = GameDataManager.CurrentMap;
        currentSave.mapScrollValue = GameDataManager.MapScrollValue;
        currentSave.preparationRoomStage = GameDataManager.PreparationRoomStage;
        currentSave.hasSkillIDs = GameDataManager.HasSkillIDs.ToList();
        currentSave.gold = GameDataManager.Gold;
        currentSave.gear = GameDataManager.Gear;
        
        // 存儲升級等級
        currentSave.powerUpLevel_MaxBlood = GameDataManager.GetPowerUpLevel(PowerUpType.MaxBlood);
        currentSave.powerUpLevel_DiceCount = GameDataManager.GetPowerUpLevel(PowerUpType.DiceCount);
        currentSave.powerUpLevel_KeepDiceCount = GameDataManager.GetPowerUpLevel(PowerUpType.KeepDiceCount);
        currentSave.powerUpLevel_MaxRollCount = GameDataManager.GetPowerUpLevel(PowerUpType.MaxRollCount);
        currentSave.unlockedAffinityStages = GameDataManager.unlockedAffinityStages.ToList();
        currentSave.charactersAffinity = GetSafeAffinityArray(GameDataManager.charactersAffinity);
        currentSave.safeRoomLevel = GameDataManager.SafeRoomLevel;
        try
        {
            currentSave.SaveTime = System.DateTime.Now;
            string json = JsonUtility.ToJson(currentSave, true);
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"{saveType}成功: " + filePath);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{saveType}失敗: " + e.Message);
            return false;
        }
    }

    // 统一读档逻辑
    private static bool LoadFromFile(string filePath, string saveType)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                currentSave = JsonUtility.FromJson<GameSaveData>(json);
                GameDataManager.PlayerName = currentSave.playerName;
                GameDataManager.PlayerData.LoadFromSaveData(currentSave.playerData);
                GameDataManager.CurrentStage = currentSave.currentStage;
                GameDataManager.CurrentMap = currentSave.currentMap;
                GameDataManager.MapScrollValue = currentSave.mapScrollValue;
                GameDataManager.HasSkillIDs = new HashSet<int>(currentSave.hasSkillIDs);
                GameDataManager.Gold = currentSave.gold;
                GameDataManager.Gear = currentSave.gear;
                GameDataManager.PreparationRoomStage = currentSave.preparationRoomStage;
                
                // 讀取升級等級
                GameDataManager.SetPowerUpLevel(PowerUpType.MaxBlood, currentSave.powerUpLevel_MaxBlood);
                GameDataManager.SetPowerUpLevel(PowerUpType.DiceCount, currentSave.powerUpLevel_DiceCount);
                GameDataManager.SetPowerUpLevel(PowerUpType.KeepDiceCount, currentSave.powerUpLevel_KeepDiceCount);
                GameDataManager.SetPowerUpLevel(PowerUpType.MaxRollCount, currentSave.powerUpLevel_MaxRollCount);
                GameDataManager.unlockedAffinityStages = new HashSet<string>(currentSave.unlockedAffinityStages ?? new List<string>());
                GameDataManager.charactersAffinity = GetSafeAffinityArray(currentSave.charactersAffinity);
                GameDataManager.SafeRoomLevel = currentSave.safeRoomLevel; 
                
                Debug.Log($"{saveType}讀檔成功");
                return true;
            }
            else
            {
                Debug.Log($"找不到{saveType}檔案");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{saveType}讀檔失敗: " + e.Message);
            return false;
        }
    }

    private static int[] GetSafeAffinityArray(int[] source)
    {
        const int affinityRoleCount = 4;
        int[] result = new int[affinityRoleCount];
        if (source == null)
        {
            return result;
        }

        int copyLength = Mathf.Min(source.Length, affinityRoleCount);
        System.Array.Copy(source, result, copyLength);
        return result;
    }

    // ==================== 全域解鎖資料（所有存檔共用） ====================

    private static string globalUnlockFilePath => Application.persistentDataPath + "/global_unlock.json";
    private static GlobalUnlockData globalUnlockData = new GlobalUnlockData();

    /// <summary>
    /// 儲存全域解鎖資料
    /// </summary>
    public static void SaveGlobalUnlock()
    {
        try
        {
            globalUnlockData.unlocked_H_Stages = GameDataManager.unlocked_H_Stages.ToList();
            string json = JsonUtility.ToJson(globalUnlockData, true);
            System.IO.File.WriteAllText(globalUnlockFilePath, json);
            Debug.Log("全域解鎖資料儲存成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError("全域解鎖資料儲存失敗: " + e.Message);
        }
    }

    /// <summary>
    /// 讀取全域解鎖資料（遊戲啟動時呼叫）
    /// </summary>
    public static void LoadGlobalUnlock()
    {
        try
        {
            if (System.IO.File.Exists(globalUnlockFilePath))
            {
                string json = System.IO.File.ReadAllText(globalUnlockFilePath);
                globalUnlockData = JsonUtility.FromJson<GlobalUnlockData>(json);
                GameDataManager.unlocked_H_Stages = new HashSet<string>(globalUnlockData.unlocked_H_Stages ?? new List<string>());
                Debug.Log($"全域解鎖資料讀取成功，已解鎖 H 關卡數: {GameDataManager.unlocked_H_Stages.Count}");
            }
            else
            {
                Debug.Log("找不到全域解鎖檔案，使用預設值");
                GameDataManager.unlocked_H_Stages = new HashSet<string>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("全域解鎖資料讀取失敗: " + e.Message);
            GameDataManager.unlocked_H_Stages = new HashSet<string>();
        }
    }
}

