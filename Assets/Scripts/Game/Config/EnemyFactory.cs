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
                enemyName = "史萊姆",
                goldReward = 10,
                maxBlood = 50f,
                currentBlood = 50f,
                diceSides = new int[] { 1, 2, 3 },
                diceCount = 10,
                maxRollCount = 1,
                skillIDs = new int[] { 1, 2, 3 },
                initialBuffs = new BuffSeed[] { }
            }
        },
        {
            2, new EnemyConfigData
            {
                enemyId = 2,
                enemyName = "哥布林",
                goldReward = 20,
                maxBlood = 100f,
                currentBlood = 100f,
                diceSides = new int[] { 1, 2, 3, 4, 5 },
                diceCount = 4,
                maxRollCount = 1,
                skillIDs = new int[] { 4 },
                initialBuffs = new BuffSeed[] { }
            }
        },
        {
            100, new EnemyConfigData
            {
                enemyId = 103,
                enemyName = "狼女",
                goldReward = 50,
                maxBlood = 150f,
                currentBlood = 15f,
                diceSides = new int[] { 1, 2, 3, 4 },
                diceCount = 2,
                maxRollCount = 1,
                skillIDs = new int[] { 5 },
                initialBuffs = new BuffSeed[] { }
            }
        }
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

public class EnemyData : BaseCharacterData
{
    [Header("基本資訊")]
    public int enemyId = 0;
    public string enemyName = "敵人";
    public int goldReward = 0;

    // 預設建構子
    public EnemyData() { }
    
    // 使用配置數據建構
    public EnemyData(int enemyId)
    {
        var config = EnemyDatabase.GetEnemyConfig(enemyId);
        ApplyConfig(config);
    }
    
    public EnemyData(EnemyConfigData config)
    {
        ApplyConfig(config);
    }
    
    // 套用配置
    protected void ApplyConfig(EnemyConfigData config)
    {
        enemyId = config.enemyId;
        enemyName = config.enemyName;
        goldReward = config.goldReward;
        maxBlood = config.maxBlood;
        currentBlood = config.currentBlood;
        diceSides = config.diceSides ?? new int[] { };
        diceCount = config.diceCount;
        maxRollCount = config.maxRollCount;
        
        // 載入技能
        skillData = new List<ISkillData>();
        if (config.skillIDs != null)
        {
            foreach (var skillId in config.skillIDs)
            {
                skillData.Add(new BaseSkill(skillId));
            }
        }
        
        // 載入初始 Buff
        buffData = new List<IBuffData>();
        if (config.initialBuffs != null)
        {
            foreach (var buffSeed in config.initialBuffs)
            {
                var buff = BuffFactory.CreateBuff(buffSeed.buffID, buffSeed.usageCount, buffSeed.duration);
                if (buff != null)
                {
                    buffData.Add(buff);
                }
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        //從skillData最後面的開始使用技能 成功使出一個技能就結束
        for (int i = skillData.Count - 1; i >= 0; i--)
        {
            skillData[i].diceBox = rollDiceResult;
            skillData[i].Use(false);
            if (skillData[i].canUseSkill())
                break;
        }
    }
}
