using System.Collections.Generic;

public class EnemyData : BaseCharacterData
{
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
                var buff = new BaseBuff(buffSeed.buffID, buffSeed.usageCount, buffSeed.duration);
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

        for (int i = 0; i < skillData.Count; i++)
        {
            skillData[i].diceBox = rollDiceResult;
            if (skillData[i].canUseSkill())
            {
                skillData[i].Use(false);
            }
        }
    }
}