using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyConfig
{
    public int enemyId;
    public string enemyName;
    public string prefabPath;
    public System.Type dataType;
}

public static class EnemyFactory
{
    private static readonly Dictionary<int, EnemyConfig> enemyConfigs = 
        new Dictionary<int, EnemyConfig>();
    
    // 靜態初始化
    static EnemyFactory()
    {
        InitializeEnemies();
    }
    
    static void InitializeEnemies()
    {
        // 註冊所有敵人
        RegisterEnemyType<SlimeData>(1, "史萊姆", "character/Slime");
        RegisterEnemyType<WolfData>(2, "狼", "character/Wolf");
    }
    
    static void RegisterEnemyType<T>(int id, string name, string prefabPath) where T : ICharacterData, new()
    {
        enemyConfigs[id] = new EnemyConfig
        {
            enemyId = id,
            enemyName = name,
            prefabPath = prefabPath,
            dataType = typeof(T)
        };
    }
    
    public static ICharacterData CreateEnemy(int enemyId)
    {
        if (enemyConfigs.TryGetValue(enemyId, out var config))
        {
            // 使用反射創建敵人實例
            return (ICharacterData)System.Activator.CreateInstance(config.dataType);
        }
        
        Debug.LogWarning($"未找到敵人 ID: {enemyId}");
        return new SlimeData();
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