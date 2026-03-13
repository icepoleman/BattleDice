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
                break;
        }

        if (skill.selfBuffs != null && skill.selfBuffs.Length > 0)
        {
            foreach (var buff in skill.selfBuffs)
            {
                // icon 還沒畫完，先都用 0 的圖
                int buffIconID = 0; // buff.buffID
                skill.effectText += "\n" + LanguageManager.GetFormat("T_Skill_selfBuff", buff.buffID, buffIconID, buff.duration);
            }
        }

        if (skill.targetBuffs != null && skill.targetBuffs.Length > 0)
        {
            foreach (var buff in skill.targetBuffs)
            {
                // icon 還沒畫完，先都用 0 的圖
                int buffIconID = 0; // buff.buffID
                skill.effectText += "\n" + LanguageManager.GetFormat("T_Skill_targetBuff", buff.buffID, buffIconID, buff.duration);
            }
        }
    }
}
