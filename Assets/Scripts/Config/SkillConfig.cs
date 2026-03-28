using System.Collections.Generic;

// 技能配置工廠 - 統一方法，適合 CSV 載入
public static class SkillFactory
{
    /// <summary>
    /// 統一創建技能的方法
    /// </summary>
    /// <param name="skillID">技能ID</param>
    /// <param name="skillName">技能名稱</param>
    /// <param name="skillType">技能類型 (Attack/Heal/Buff)</param>
    /// <param name="requirementType">需求類型</param>
    /// <param name="needDices">需要的骰子資料 (格式依類型不同，見下方說明)</param>
    /// <param name="skillValue">技能數值</param>
    /// <param name="conditionText">條件文字</param>
    /// <param name="effectText">效果文字</param>
    /// <param name="selfBuffs">自身Buff</param>
    /// <param name="targetBuffs">目標Buff</param>
    /// <param name="generateDices">生成骰子資料 允許骰子, 最後一個=需要數量 (1|3|5|3)</param>
    /// <param name="tag">標記技能出處</param>
    /// <remarks>
    /// needDices 格式:
    /// - SpecificDices: 特定骰子 (1|2|3)
    /// - SameDices: [0]=需要數量 (2)
    /// - DiceSum: [0]=需要總和 (5)
    /// - SpecificDicesWithRepeat: 允許骰子, 最後一個=需要數量 (1|3|5|3)
    /// - ConsecutiveDices: [0]=需要連續數量 (3)
    /// </remarks>
    public static SkillConfigData Create(
        int skillID,
        string skillName,
        SkillType skillType,
        SkillRequirementType requirementType,
        int[] needDices = null,
        int skillValue = 0,
        string conditionText = "",
        string effectText = "",
        BuffSeed[] selfBuffs = null,
        BuffSeed[] targetBuffs = null,
        int breakDiceCount = 0,
        int[] generateDices = null,
        string tag = "",
        int price = 0,
        string modifierConditions="",
        string modifierEffects=""   
        )
    {
        return new SkillConfigData
        {
            skillID = skillID,
            skillName = skillName,
            skillType = skillType,
            requirementType = requirementType,
            needDicesData = needDices ?? new int[] { },
            skillValue = skillValue,
            conditionText = conditionText,
            effectText = effectText,
            selfBuffs = selfBuffs,
            targetBuffs = targetBuffs,
            breakDiceCount = breakDiceCount,
            generateDicesData = generateDices ?? new int[] { },
            tag = tag,
            price = price,
            modifierConditions = modifierConditions,
            modifierEffects = modifierEffects
        };
    }
}

