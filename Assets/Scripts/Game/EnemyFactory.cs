using System.Collections.Generic;
using UnityEngine;

public static class EnemyFactory
{
    private static readonly Dictionary<int, EnemyConfig> enemyConfigs =
        new Dictionary<int, EnemyConfig>();

    static EnemyFactory()
    {
        InitializeEnemies();
    }

    static void InitializeEnemies()
    {
        // 直接註冊配置，不需要泛型
        RegisterEnemy(new EnemyConfig
        {
            enemyId = 1,
            enemyName = "史萊姆",
            maxHP = 50f,
            skillIds = new List<SkillID> { SkillID.Punch },
            prefabPath = "character/Slime",
            diceSides = new int[] { 1, 2 },
            diceCount = 2,
            goldReward = 500
        });

        RegisterEnemy(new EnemyConfig
        {
            enemyId = 2,
            enemyName = "哥布林",
            maxHP = 80f,
            skillIds = new List<SkillID> { SkillID.Kaminari },
            prefabPath = "character/Goblin",
            diceSides = new int[] { 1, 2 },
            diceCount = 2,
            goldReward = 800
        });
    }

    static void RegisterEnemy(EnemyConfig config)
    {
        enemyConfigs[config.enemyId] = config;
        Debug.Log($"註冊敵人: {config.enemyName} (ID: {config.enemyId})");
    }

    // 創建敵人 - 不再需要反射！
    public static EnemyData CreateEnemy(int enemyId)
    {
        if (enemyConfigs.TryGetValue(enemyId, out var config))
        {
            var enemy = new EnemyData();
            enemy.LoadFromConfig(config);
            return enemy;
        }

        Debug.LogWarning($"未找到敵人 ID: {enemyId}，建立預設敵人");
        return CreateDefaultEnemy();
    }

    static EnemyData CreateDefaultEnemy()
    {
        var defaultConfig = new EnemyConfig
        {
            enemyId = 1,
            enemyName = "預設敵人",
            maxHP = 30f,
            diceSides = new int[] { 1, 2 },
            diceCount = 2,
            skillIds = new List<SkillID> { SkillID.Punch }
        };

        var enemy = new EnemyData();
        enemy.LoadFromConfig(defaultConfig);
        return enemy;
    }

    public static EnemyConfig GetEnemyConfig(int enemyId)
    {
        enemyConfigs.TryGetValue(enemyId, out var config);
        return config;
    }

    public static string GetEnemyName(int enemyId)
    {
        var config = GetEnemyConfig(enemyId);
        return config?.enemyName ?? "未知敵人";
    }
}