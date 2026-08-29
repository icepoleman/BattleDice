using System.Collections.Generic;
using UnityEngine;

// 敵人配置結構體
public struct EnemyConfigData
{
    // 基本資訊
    public int enemyId;              // 敵人唯一識別碼
    public string enemyName;         // 敵人名稱
    public int goldReward;           // 擊敗獎勵金幣
    public int gearReward;           // 擊敗獎勵齒輪

    // 血量配置
    public float maxBlood;           // 最大血量

    // 骰子配置 (預設 1-6)
    public int diceCount;            // 骰子數量

    // 技能與 Buff 配置
    public int[] skillIDs;           // 技能 ID 列表
    public BuffSeed[] initialBuffs;  // 初始 Buff 列表
    public string enemyType;          // 敵人類型 (例如: "Boss", "Zako" Erito)
    public int[] diceSides;
    public string imgId;            // 圖片 ID (對應資源)
    public int openStage;          // 開放關卡 (0 表示無限制) 用於隨機敵人格
        // 預設骰子面 (1-6)

    
    public float GetCurrentBlood() => maxBlood;
}

// 敵人配置資料庫 - 從 CSV 載入
public static class EnemyDatabase
{
    private static Dictionary<int, EnemyConfigData> _enemies;
    
    public static Dictionary<int, EnemyConfigData> Enemies
    {
        get
        {
            if (_enemies == null) LoadFromCSV();
            return _enemies;
        }
    }
    
    // 從 CSV 載入敵人資料
    public static void LoadFromCSV()
    {
        _enemies = CSVReader.LoadEnemyCSV("enemy");
        if (_enemies == null)
        {
            _enemies = new Dictionary<int, EnemyConfigData>();
        }
    }
    
    public static void Reload()
    {
        _enemies = null;
    }

    public static EnemyConfigData GetEnemyConfig(int enemyId)
    {
        if (Enemies.TryGetValue(enemyId, out var config))
        {
            return config;
        }
        return default;
    }
}

public static class EnemyFactory
{
    public static EnemyData CreateEnemy(int enemyId)
    {
        var config = EnemyDatabase.GetEnemyConfig(enemyId);
        if (config.enemyId == 0)
        {
            Debug.LogError("EnemyFactory: 未知的敵人 ID " + enemyId);
            return null;
        }
        return new EnemyData(config);
    }
}

