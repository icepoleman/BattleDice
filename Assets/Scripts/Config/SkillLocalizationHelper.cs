using System;
using System.Diagnostics;
using Unity.VisualScripting;

/// <summary>
/// 技能文本本地化處理工具
/// </summary>
public static class SkillLocalizationHelper
{
    /// <summary>
    /// 處理所有技能的本地化文本
    /// </summary>
    public static void LocalizeAllSkills(System.Collections.Generic.Dictionary<int, SkillConfigData> skills)
    {
        var keys = new System.Collections.Generic.List<int>(skills.Keys);
        foreach (var key in keys)
        {
            var skill = skills[key];
            LocalizeCondition(ref skill);
            LocalizeEffect(ref skill);
            skills[key] = skill;
        }
    }

    /// <summary>
    /// 處理單一技能的本地化
    /// </summary>
    public static void LocalizeSkill(ref SkillConfigData skill)
    {
        LocalizeCondition(ref skill);
        LocalizeEffect(ref skill);
    }

    /// <summary>
    /// 技能條件文本本地化
    /// </summary>
    public static void LocalizeCondition(ref SkillConfigData skill)
    {
        switch (skill.requirementType)
        {
            case SkillRequirementType.SpecificDices:
                // 不動作，直接顯示在說明裡面
                break;
            case SkillRequirementType.SameDices:
                skill.conditionText = LanguageManager.GetFormat("T_Skill_SameDices", skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.DiceSum:
                // 不動作，直接顯示在說明裡面
                break;
            case SkillRequirementType.SpecificDicesWithRepeat:
                if (skill.needDicesData[0] == 1)
                    skill.conditionText = LanguageManager.GetFormat("T_Skill_SpecificDicesWithRepeat_low", skill.GetNeedDiceNum());
                else
                    skill.conditionText = LanguageManager.GetFormat("T_Skill_SpecificDicesWithRepeat_high", skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.ConsecutiveDices:
                skill.conditionText = LanguageManager.GetFormat("T_Skill_ConsecutiveDices", skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.AnyDices:
                skill.conditionText = LanguageManager.GetFormat("T_Skill_AnyDices", skill.GetNeedDiceNum());
                break;
        }
    }

    /// <summary>
    /// 技能效果文本本地化
    /// </summary>
    public static void LocalizeEffect(ref SkillConfigData skill)
    {
        switch (skill.skillType)
        {
            case SkillType.Attack:
                skill.effectText = LanguageManager.GetFormat("T_Skill_SkillType_atk", skill.skillValue);
                break;
            case SkillType.Heal:
                skill.effectText = LanguageManager.GetFormat("T_Skill_SkillType_heal", skill.skillValue);
                break;
            case SkillType.Buff:
                skill.effectText = "";
                break;
        }

        if (skill.selfBuffs != null && skill.selfBuffs.Length > 0)
        {
            foreach (var buff in skill.selfBuffs)
            {
                // icon 還沒畫完，先都用 0 的圖
                int buffIconID = 0; // buff.buffID
                AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_selfBuff", buff.buffID, buffIconID, buff.duration));
            }
        }

        if (skill.targetBuffs != null && skill.targetBuffs.Length > 0)
        {
            foreach (var buff in skill.targetBuffs)
            {
                // icon 還沒畫完，先都用 0 的圖
                int buffIconID = 0; // buff.buffID
                AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_targetBuff", buff.buffID, buffIconID, buff.duration));
            }
        }
        if (skill.breakDiceCount > 0)//怪物combo
        {
            AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_enemyCombo", skill.breakDiceCount));
        }
        if (skill.generateDicesData.Length > 0)//生成骰子
        {
            int diceCount = skill.generateDicesData.Length;//這裡用數量區分類型
            int burnCount = skill.generateDicesData[skill.generateDicesData.Length - 1];
            Console.WriteLine(LanguageManager.GetFormat("T_Skill_generateDices_spDice", burnCount));
            if (skill.generateDicesData[0] == 0)
                AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_generateDices_spDice", burnCount));
            if (diceCount == 4)
            {
                if (skill.generateDicesData[0] == 1)
                    AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_generateDices_lowDice", burnCount));
                if (skill.generateDicesData[0] == 4)
                    AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_generateDices_highDice", burnCount));
            }
            if (diceCount == 7)
                AppendEffectText(ref skill, LanguageManager.GetFormat("T_Skill_generateDices_randomDice", burnCount));
        }
    }

    /// <summary>
    /// 附加效果文本，如果 effectText 為空則直接設定，否則換行後附加
    /// </summary>
    private static void AppendEffectText(ref SkillConfigData skill, string text)
    {
        if (string.IsNullOrEmpty(skill.effectText))
            skill.effectText = text;
        else
            skill.effectText += "\n" + text;
    }
}
