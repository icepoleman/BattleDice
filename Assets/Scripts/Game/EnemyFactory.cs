using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Game/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyConfig> enemies = new List<EnemyConfig>();
    
    private Dictionary<int, EnemyConfig> enemyLookup;
    
    void OnEnable()
    {
        BuildLookupTable();
    }
    
    void BuildLookupTable()
    {
        enemyLookup = new Dictionary<int, EnemyConfig>();
        foreach (var enemy in enemies)
        {
            enemyLookup[enemy.enemyId] = enemy;
        }
    }
    
    public EnemyData CreateEnemy(int enemyId)
    {
        if (enemyLookup == null) BuildLookupTable();
        
        if (enemyLookup.TryGetValue(enemyId, out var config))
        {
            var enemy = new EnemyData();
            enemy.LoadFromConfig(config);
            return enemy;
        }
        
        return null;
    }
}

// 工廠改為使用 ScriptableObject
public static class EnemyFactory
{
    private static EnemyDatabase database;
    
    public static EnemyDatabase Database
    {
        get
        {
            if (database == null)
            {
                database = Resources.Load<EnemyDatabase>("EnemyDatabase");
                if (database == null)
                {
                    Debug.LogError("找不到 EnemyDatabase！創建預設資料庫");
                }
            }
            return database;
        }
    }
    
    public static EnemyData CreateEnemy(int enemyId)
    {
        var enemy = Database?.CreateEnemy(enemyId);
        if (enemy == null)
        {
            Debug.LogWarning($"無法創建敵人 ID: {enemyId}，使用預設敵人");
            enemy = CreateDefaultEnemy();
        }
        return enemy;
    }
    
    // 創建預設敵人（測試用）
    static EnemyData CreateDefaultEnemy()
    {
        var enemy = new EnemyData();
        var defaultConfig = new EnemyConfig
        {
            enemyId = 1,
            enemyName = "測試史萊姆",
            maxHP = 50f,
            skillIds = new List<SkillID> { SkillID.Punch },
            prefabPath = "character/Slime",
            goldReward = 5
        };
        
        enemy.LoadFromConfig(defaultConfig);
        return enemy;
    }
}