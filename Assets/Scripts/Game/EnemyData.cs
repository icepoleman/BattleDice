using System.Collections.Generic;
using System.Threading.Tasks;

public class EnemyData : BaseCharacterData
{
    public int enemyId = 0;
    public string enemyName = "敵人";
    public int goldReward = 0;
    
    // 技能施放間隔（毫秒）
    public int skillCastInterval = 800;

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
    public void DistroyTargetDice(List<int> targetIndices)
    {
        // 移除指定索引的骰子
        targetIndices.Sort();
        targetIndices.Reverse(); // 反向排序以避免索引錯亂

        foreach (int index in targetIndices)
        {
            if (index >= 0 && index < rollDiceResult.Count)
            {
                rollDiceResult.RemoveAt(index);
            }
        }
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

        // 載入怪物技能
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

    public override async void UseSkill()
    {
        base.UseSkill();

        // 取得可發動的技能列表（骰子消耗不重複）
        var usableSkills = GetUsableSkills(rollDiceResult);
        
        for (int i = 0; i < usableSkills.Count; i++)
        {
            var skillInfo = usableSkills[i];
            skillInfo.skill.diceBox = new List<int>(skillInfo.usedDices);
            skillInfo.skill.UseSkill(false);
            
            // 如果不是最後一個技能，等待間隔
            if (i < usableSkills.Count - 1)
            {
                await Task.Delay(skillCastInterval);
            }
        }
    }
    
    // 給定骰子結果，計算哪些技能可以發動（骰子不重複消耗）
    public List<SkillUseInfo> GetUsableSkills(List<int> diceResult)
    {
        List<SkillUseInfo> usableSkills = new List<SkillUseInfo>();
        List<int> remainingDice = new List<int>(diceResult);
        
        // 依序檢查每個技能
        foreach (var skill in skillData)
        {
            if (skill.CanUseWithDice(remainingDice))
            {
                // 取得這個技能會消耗的骰子
                List<int> usedDices = skill.GetUsedDices(remainingDice);
                
                // 從剩餘骰子中移除已使用的
                foreach (int dice in usedDices)
                {
                    remainingDice.Remove(dice);
                }
                
                usableSkills.Add(new SkillUseInfo(skill, usedDices));
            }
        }
        
        return usableSkills;
    }
}

// 技能使用資訊
public class SkillUseInfo
{
    public ISkillData skill;
    public List<int> usedDices;// 使用的骰子列表
    
    public SkillUseInfo(ISkillData skill, List<int> usedDices)
    {
        this.skill = skill;
        this.usedDices = usedDices;
    }
}