using System.Collections.Generic;
using UnityEngine;

public class EnemyData
{
    [Header("基本資訊")]
    public int enemyId = 0;
    public string enemyName = "敵人";
    public float maxHP { get; set; }
    public float currentHP { get; set; }
    public int[] diceSides { get; set; }
    public int diceCount { get; set; }

    [Header("技能配置")]
    public List<SkillID> skillIds = new List<SkillID>();
    public List<ISkillData> skillData { get; set; } = new List<ISkillData>();

    [Header("視覺配置")]
    public string prefabPath = "";

    public List<int> rollDiceResult { get; set; } = new List<int>();

    // 從配置載入資料
    public void LoadFromConfig(EnemyConfig config)
    {
        enemyId = config.enemyId;
        enemyName = config.enemyName;
        maxHP = config.maxHP;
        diceSides = config.diceSides;
        diceCount = config.diceCount;
        currentHP = config.maxHP;
        skillIds = config.skillIds;
        prefabPath = config.prefabPath;
        // 載入技能
        skillData.Clear();
        foreach (var skillId in config.skillIds)
        {
            var skill = SkillFactory.CreateSkill(skillId);
            if (skill != null)
            {
                skillData.Add(skill);
            }
        }
    }

    // 實作 ICharacterData 介面
    public bool IsDead() => currentHP <= 0;

    public void TakeDamage(float damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public List<int> RollDice()
    {
        rollDiceResult.Clear(); // 清空之前的結果
        for (int i = 0; i < diceCount; i++)
        {
            int side = diceSides[Random.Range(0, diceSides.Length)];
            rollDiceResult.Add(side);
        }
        return rollDiceResult;
    }
}

[System.Serializable]
public class EnemyConfig
{
    [Header("基本資訊")]
    public int enemyId;
    public string enemyName;
    public float maxHP = 100f;
    [Header("骰子設定")]
    public int[] diceSides;
    public int diceCount;
    public List<int> rollDiceResult { get; set; } = new List<int>();
    
    [Header("技能配置")]
    public List<SkillID> skillIds = new List<SkillID>();
    
    [Header("視覺設定")]
    public string prefabPath;
    //public string portraitPath; // 用於 Resources.Load
    
    [Header("戰利品")]
    //public int expReward = 10;
    public int goldReward = 100;
    //public List<ItemDrop> itemDrops = new List<ItemDrop>();
}
