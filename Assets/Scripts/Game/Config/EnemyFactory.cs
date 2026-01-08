using System.Collections.Generic;
using UnityEngine;

// 敵人配置結構體
public struct EnemyConfigData
{
    // 基本資訊
    public int enemyId;              // 敵人唯一識別碼
    public string enemyName;         // 敵人名稱
    public int goldReward;           // 擊敗獎勵金幣

    // 血量配置
    public float maxBlood;           // 最大血量
    public float currentBlood;       // 當前血量

    // 骰子配置
    public int[] diceSides;          // 可用的骰子面
    public int diceCount;            // 骰子數量
    public int maxRollCount;         // 最大擲骰次數

    // 技能與 Buff 配置
    public int[] skillIDs;           // 技能 ID 列表
    public BuffSeed[] initialBuffs;  // 初始 Buff 列表
}

// 敵人配置資料庫
public static class EnemyDatabase
{
    public static readonly Dictionary<int, EnemyConfigData> Enemies = new Dictionary<int, EnemyConfigData>
    {
        {
            1, new EnemyConfigData
            {
                enemyId = 1,
                enemyName = "鴨鴨",
                goldReward = 1000,
                maxBlood = 50f,
                currentBlood = 50f,
                diceSides = new int[] { 1, 2,3,4,5,6 },
                diceCount = 1,
                skillIDs = new int[] { 107 },
                initialBuffs = new BuffSeed[]
                {

                }
            }
        },
        {
            2, new EnemyConfigData
            {
                enemyId = 2,
                enemyName = "狼人",
                goldReward = 20,
                maxBlood = 60f,
                currentBlood = 60f,
                diceSides = new int[] { 1, 2, 3, 4, 5, 6 },
                diceCount = 4,
                skillIDs = new int[] { 104,103 },
                initialBuffs = new BuffSeed[] { }
            }
        },
        {
            3, new EnemyConfigData
            {
                enemyId = 3,
                enemyName = "邪惡鴨鴨",
                goldReward = 50,
                maxBlood = 90f,
                currentBlood = 90f,
                diceSides = new int[] { 3, 4,5,6 },
                diceCount = 4,
                maxRollCount = 1,
                skillIDs = new int[] { 106,105 },
                initialBuffs = new BuffSeed[]
                {

                }
            }
        },
        {
            4, new EnemyConfigData
            {
                enemyId = 4,
                enemyName = "哥布林勇士",
                goldReward = 50,
                maxBlood = 80f,
                currentBlood = 80f,
                diceSides = new int[] {3, 4,5,6 },
                diceCount = 4,
                maxRollCount = 1,
                skillIDs = new int[] { 9,5 },
            }
        },
        {
            5, new EnemyConfigData
            {
                enemyId = 5,
                enemyName = "獨眼巨人",
                goldReward = 50,
                maxBlood = 150f,
                currentBlood = 150f,
                diceSides = new int[] {1,2,3, 4,5 },
                diceCount = 6,
                maxRollCount = 1,
                skillIDs = new int[] { 16,4 },
            }
        },
        {
            6, new EnemyConfigData
            {
                enemyId = 6,
                enemyName = "魔女",
                goldReward = 50,
                maxBlood = 220,
                currentBlood = 220,
                diceSides = new int[] {1,2,3 ,4,5,6},
                diceCount = 8,
                maxRollCount = 1,
                skillIDs = new int[] { 1,13,2 },
                initialBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 4, usageCount = 3, duration = 0 } ,
                    new BuffSeed { buffID = 11, usageCount = 0, duration = 3 } ,
                }
            }
        },
    };

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

