using System;
using System.Collections.Generic;

/// <summary>
/// 可升級的屬性類型
/// </summary>
public enum PowerUpType
{
    MaxBlood,       // 最大血量
    DiceCount,      // 骰子數量
    KeepDiceCount,  // 保留骰子數量
    MaxRollCount    // 最大重骰次數
}

/// <summary>
/// 單一等級的升級配置
/// </summary>
[Serializable]
public struct PowerUpLevelData
{
    public int gearCost;        // 升級所需齒輪
    public float increaseValue; // 升級增加的數值
    
    public PowerUpLevelData(int cost, float value)
    {
        gearCost = cost;
        increaseValue = value;
    }
}

/// <summary>
/// 升級配置資料
/// </summary>
[Serializable]
public struct PowerUpConfigData
{
    public PowerUpType type;           // 升級類型
    public string displayName;         // 顯示名稱
    public string description;         // 描述
    public int maxLevel;               // 最大等級
    public PowerUpLevelData[] levels;  // 每個等級的配置
}

/// <summary>
/// 升級系統配置與管理
/// </summary>
public static class PowerUpDatabase
{
    private static Dictionary<PowerUpType, PowerUpConfigData> configs;
    
    static PowerUpDatabase()
    {
        InitializeConfigs();
    }
    
    private static void InitializeConfigs()
    {
        configs = new Dictionary<PowerUpType, PowerUpConfigData>
        {
            // 最大血量升級配置
            {
                PowerUpType.MaxBlood,
                new PowerUpConfigData
                {
                    type = PowerUpType.MaxBlood,
                    displayName = "T_PowerUp_MaxBlood",
                    description = "T_PowerUp_MaxBlood_Desc",
                    maxLevel = 5,
                    levels = new PowerUpLevelData[]
                    {
                        new PowerUpLevelData(1, 20),   // Lv1: 花費2齒輪，+20血量
                        new PowerUpLevelData(3, 20),   // Lv2: 花費3齒輪，+20血量
                        new PowerUpLevelData(5, 30),   // Lv3: 花費5齒輪，+30血量
                        new PowerUpLevelData(8, 30),   // Lv4: 花費8齒輪，+30血量
                        new PowerUpLevelData(12, 50),  // Lv5: 花費12齒輪，+50血量
                    }
                }
            },
            // 骰子數量升級配置
            {
                PowerUpType.DiceCount,
                new PowerUpConfigData
                {
                    type = PowerUpType.DiceCount,
                    displayName = "T_PowerUp_DiceCount",
                    description = "T_PowerUp_DiceCount_Desc",
                    maxLevel = 3,
                    levels = new PowerUpLevelData[]
                    {
                        new PowerUpLevelData(5, 1),    // Lv1: 花費5齒輪，+1骰子
                        new PowerUpLevelData(10, 1),   // Lv2: 花費10齒輪，+1骰子
                        new PowerUpLevelData(15, 1),   // Lv3: 花費15齒輪，+1骰子
                    }
                }
            },
            // 保留骰子數量升級配置
            {
                PowerUpType.KeepDiceCount,
                new PowerUpConfigData
                {
                    type = PowerUpType.KeepDiceCount,
                    displayName = "T_PowerUp_KeepDiceCount",
                    description = "T_PowerUp_KeepDiceCount_Desc",
                    maxLevel = 4,
                    levels = new PowerUpLevelData[]
                    {
                        new PowerUpLevelData(2, 1),    // Lv1: 花費2齒輪，+1保留
                        new PowerUpLevelData(4, 1),    // Lv2: 花費4齒輪，+1保留
                        new PowerUpLevelData(6, 1),   // Lv3: 花費6齒輪，+1保留
                        new PowerUpLevelData(8, 1),   // Lv4: 花費8齒輪，+1保留
                    }
                }
            },
            // 最大重骰次數升級配置
            {
                PowerUpType.MaxRollCount,
                new PowerUpConfigData
                {
                    type = PowerUpType.MaxRollCount,
                    displayName = "T_PowerUp_MaxRollCount",
                    description = "T_PowerUp_MaxRollCount_Desc",
                    maxLevel = 4,
                    levels = new PowerUpLevelData[]
                    {
                        new PowerUpLevelData(4, 1),    // Lv1: 花費4齒輪，+1次重骰
                        new PowerUpLevelData(8, 1),    // Lv2: 花費8齒輪，+1次重骰
                        new PowerUpLevelData(12, 1),   // Lv3: 花費12齒輪，+1次重骰
                        new PowerUpLevelData(16, 1),   // Lv4: 花費16齒輪，+1次重骰
                    }
                }
            }
        };
    }
    
    /// <summary>
    /// 取得指定類型的升級配置
    /// </summary>
    public static PowerUpConfigData GetConfig(PowerUpType type)
    {
        if (configs.TryGetValue(type, out var config))
        {
            return config;
        }
        return default;
    }
    
    /// <summary>
    /// 取得所有升級配置
    /// </summary>
    public static Dictionary<PowerUpType, PowerUpConfigData> GetAllConfigs()
    {
        return configs;
    }
}

/// <summary>
/// 升級系統管理器
/// </summary>
public static class PowerUpManager
{
    /// <summary>
    /// 取得玩家目前的升級等級
    /// </summary>
    public static int GetCurrentLevel(PowerUpType type)
    {
        return GameDataManager.GetPowerUpLevel(type);
    }
    
    /// <summary>
    /// 取得玩家目前的屬性值
    /// </summary>
    public static float GetCurrentValue(PowerUpType type)
    {
        var player = GameDataManager.PlayerData;
        return type switch
        {
            PowerUpType.MaxBlood => player.maxBlood,
            PowerUpType.DiceCount => player.diceCount,
            PowerUpType.KeepDiceCount => player.keepDiceCount,
            PowerUpType.MaxRollCount => player.maxRollCount,
            _ => 0
        };
    }
    
    /// <summary>
    /// 取得下一級升級所需的齒輪數量
    /// </summary>
    /// <returns>所需齒輪，-1 表示已滿級</returns>
    public static int GetNextLevelCost(PowerUpType type)
    {
        var config = PowerUpDatabase.GetConfig(type);
        int currentLevel = GetCurrentLevel(type);
        
        if (currentLevel >= config.maxLevel)
            return -1; // 已滿級
            
        return config.levels[currentLevel].gearCost;
    }
    
    /// <summary>
    /// 取得下一級升級增加的數值
    /// </summary>
    /// <returns>增加數值，-1 表示已滿級</returns>
    public static float GetNextLevelIncrease(PowerUpType type)
    {
        var config = PowerUpDatabase.GetConfig(type);
        int currentLevel = GetCurrentLevel(type);
        
        if (currentLevel >= config.maxLevel)
            return -1; // 已滿級
            
        return config.levels[currentLevel].increaseValue;
    }
    
    /// <summary>
    /// 檢查是否可以升級
    /// </summary>
    public static bool CanUpgrade(PowerUpType type)
    {
        int cost = GetNextLevelCost(type);
        if (cost < 0) return false; // 已滿級
        
        return GameDataManager.Gear >= cost;
    }
    
    /// <summary>
    /// 檢查是否已達最大等級
    /// </summary>
    public static bool IsMaxLevel(PowerUpType type)
    {
        var config = PowerUpDatabase.GetConfig(type);
        return GetCurrentLevel(type) >= config.maxLevel;
    }
    
    /// <summary>
    /// 執行升級
    /// </summary>
    /// <returns>是否升級成功</returns>
    public static bool TryUpgrade(PowerUpType type)
    {
        if (!CanUpgrade(type))
            return false;
            
        var config = PowerUpDatabase.GetConfig(type);
        int currentLevel = GetCurrentLevel(type);
        int cost = config.levels[currentLevel].gearCost;
        float increase = config.levels[currentLevel].increaseValue;
        
        // 扣除齒輪
        GameDataManager.Gear -= cost;
        
        // 增加等級
        GameDataManager.SetPowerUpLevel(type, currentLevel + 1);
        
        // 套用屬性提升
        ApplyStatIncrease(type, increase);
        
        // 自動存檔
        SaveManager.AutoSave();
        
        UnityEngine.Debug.Log($"升級成功: {type} Lv{currentLevel + 1}，消耗 {cost} 齒輪，增加 {increase}");
        
        return true;
    }
    
    /// <summary>
    /// 套用屬性提升
    /// </summary>
    private static void ApplyStatIncrease(PowerUpType type, float increase)
    {
        var player = GameDataManager.PlayerData;
        
        switch (type)
        {
            case PowerUpType.MaxBlood:
                player.maxBlood += increase;
                player.currentBlood += increase; // 升級時同時回復增加的血量
                break;
            case PowerUpType.DiceCount:
                player.diceCount += (int)increase;
                player.manaRollerMaxDiceCount += (int)increase;
                break;
            case PowerUpType.KeepDiceCount:
                player.keepDiceCount += (int)increase;
                break;
            case PowerUpType.MaxRollCount:
                player.maxRollCount += (int)increase;
                break;
        }
    }
}
