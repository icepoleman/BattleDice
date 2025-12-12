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
                effectText = "造成20點傷害",
                skillValue = 20,
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
                skillName = "拳頭攻擊",
                skillType = SkillType.Attack,
                conditionText = "任意骰子1個以上",
                effectText = "造成骰子點數總和的傷害",
                skillValue = 0,
                needDicesData = new int[] { },
                acceptMoreDice = true,
                requirementType = SkillRequirementType.AnyDice,
                requiredDiceCount = 1,
                useDiceSumAsDamage = true,
                damageMultiplier = 1f,
            }
        },
        {
            5, new SkillConfigData
            {
                skillID = 5,
                skillName = "爪擊",
                skillType = SkillType.Attack,
                conditionText = "任意骰子2個以上",
                effectText = "造成骰子點數總和的兩倍傷害",
                skillValue = 0,
                needDicesData = new int[] { },
                acceptMoreDice = true,
                requirementType = SkillRequirementType.AnyDice,
                requiredDiceCount = 2,
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
        {
            8, new SkillConfigData
            {
                skillID = 8,
                skillName = "治癒光環",
                skillType = SkillType.Buff,
                conditionText = "",
                effectText = "治癒狀態3回合",
                skillValue = 0,
                needDicesData = new int[] {2,2},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                selfBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 16, usageCount = 0, duration = 3 }
                }
            }
        },
        {
            9, new SkillConfigData
            {
                skillID = 9,
                skillName = "力量!啟動!",
                skillType = SkillType.Buff,
                conditionText = "",
                effectText = "力量增幅3回合",
                skillValue = 0,
                needDicesData = new int[] {3,3},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                selfBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 6, usageCount = 0, duration = 3 }
                }
            }
        },
        {
            10, new SkillConfigData
            {
                skillID = 10,
                skillName = "4喔",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成40點傷害",
                skillValue = 40,
                needDicesData = new int[] {4,4},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                selfBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 6, usageCount = 0, duration = 3 }
                }
            }
        },
        {
            11, new SkillConfigData
            {
                skillID = 11,
                skillName = "奪魂擊",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成30點傷害,敵方畏懼1回合",
                skillValue = 30,
                needDicesData = new int[] {5,5},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                targetBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 12, usageCount = 0, duration = 1 }
                }
            }
        },
        {
            12, new SkillConfigData
            {
                skillID = 12,
                skillName = "奮力一擊",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成60點傷害",
                skillValue = 60,
                needDicesData = new int[] {6,6},
                requirementType = SkillRequirementType.SpecificDicesWithRepeat,
                requiredDiceCount = 2,
                targetBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 9, usageCount = 0, duration = 1 }
                }
            }
        },
        {
            13, new SkillConfigData
            {
                skillID = 13,
                skillName = "魔力炸彈",
                skillType = SkillType.Attack,
                conditionText = "",
                effectText = "造成50點傷害,魔力增幅3回合",
                skillValue = 50,
                needDicesData = new int[] {4,5,6},
                requirementType = SkillRequirementType.SpecificDices,
                selfBuffs = new BuffSeed[]
                {
                    new BuffSeed { buffID = 11, usageCount = 0, duration = 3 }
                }
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