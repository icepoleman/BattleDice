using System.Collections.Generic;
// 技能配置資料庫
public static class SkillDatabase
{
    public static readonly Dictionary<int, SkillConfigData> Skills = new Dictionary<int, SkillConfigData>
    {
        {
            1, new SkillConfigData
            {
                skillID = 1,
                skillName = "火球",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成80點傷害",
                skillValue = 80,
                needDicesData = new int[] { 1, 2, 3 },
                requirementType = SkillRequirementType.SpecificDices,
                damageMultiplier = 1f,
                selfBuffs = new BuffSeed[] { },
                targetBuffs = new BuffSeed[] { }
            }
        },
        {
            2, new SkillConfigData
            {
                skillID = 2,
                skillName = "雷電",
                skillType = SkillType.Attack,
                conditionText = "相同點數骰子x2",
                effectText = "造成30點傷害",
                skillValue = 30,
                needDicesData = new int[] { },
                requirementType = SkillRequirementType.SameDices,
                requiredSameCount = 2,
                damageMultiplier = 1f,
                selfBuffs = new BuffSeed[] { },
                targetBuffs = new BuffSeed[] { }
            }
        },
        {
            3, new SkillConfigData
            {
                skillID = 3,
                skillName = "風刃",
                skillType = SkillType.Attack,
                conditionText = "點數總和5以上",
                effectText = "造成10點傷害",
                skillValue = 10,
                needDicesData = new int[] { },
                requirementType = SkillRequirementType.DiceSum,
                requiredSum = 5,
                damageMultiplier = 1f,
                selfBuffs = new BuffSeed[] { },
                targetBuffs = new BuffSeed[] { }
            }
        },
        {
            4, new SkillConfigData
            {
                skillID = 4,
                skillName = "魔力拳",
                skillType = SkillType.Attack,
                conditionText = "任意骰子2個以上",
                effectText = "造成骰子點數總和的傷害",
                skillValue = 0,
                needDicesData = new int[] { },
                acceptMoreDice = true,
                requirementType = SkillRequirementType.AnyDice,
                requiredDiceCount = 2,
                useDiceSumAsDamage = true,
                damageMultiplier = 1f,
                selfBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 6, usageCount = 0, duration = 2 }
                },
                targetBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 7, usageCount = 0, duration = 3 }
                }
            }
        },
        {
            5, new SkillConfigData
            {
                skillID = 5,
                skillName = "爪擊",
                skillType = SkillType.Attack,
                conditionText = "任意骰子3個以上",
                effectText = "造成骰子點數總和的兩倍傷害",
                skillValue = 0,
                needDicesData = new int[] { },
                acceptMoreDice = true,
                requirementType = SkillRequirementType.AnyDice,
                requiredDiceCount = 3,
                useDiceSumAsDamage = true,
                damageMultiplier = 2f,
                selfBuffs = new BuffSeed[] { },
                targetBuffs = new BuffSeed[] { }
            }
        },
        {
            6, new SkillConfigData
            {
                skillID = 6,
                skillName = "毒霧咒",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成10點傷害並給予對方中毒狀態3回合",
                skillValue = 10,
                needDicesData = new int[] {1,1},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                targetBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 7, usageCount = 0, duration = 3 }
                }
            }
        },
        {
            7, new SkillConfigData
            {
                skillID = 7,
                skillName = "回復術",
                skillType = SkillType.Heal,
                conditionText = "單數骰*3",
                effectText = "生命+50",
                skillValue = 50,
                needDicesData = new int[] {1,3,5},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 3,
            }
        },
    };

    public static SkillConfigData GetSkillConfig(int skillID)
    {
        if (Skills.TryGetValue(skillID, out var config))
        {
            return config;
        }
        return default;
    }
}