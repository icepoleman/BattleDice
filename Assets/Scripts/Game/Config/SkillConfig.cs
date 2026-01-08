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
    /// <param name="breakDiceCount">破壞骰子數</param>
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
        string tag = "")
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
            tag = tag
        };
    }
}

// 技能配置資料庫 - 從 CSV 載入
public static class SkillDatabase
{
    private static Dictionary<int, SkillConfigData> _skills;
    
    public static Dictionary<int, SkillConfigData> Skills
    {
        get
        {
            if (_skills == null) LoadFromCSV();
            return _skills;
        }
    }

    // 載入所有技能 CSV（玩家 + 怪物）
    public static void LoadFromCSV()
    {
        _skills = new Dictionary<int, SkillConfigData>();
        
        // 載入玩家技能
        LoadAdditionalCSV("skill");
        
        // 載入怪物技能
        LoadAdditionalCSV("enemySkill");
    }
    
    // 載入額外的 CSV 並合併到技能庫
    public static void LoadAdditionalCSV(string fileName)
    {
        if (_skills == null) _skills = new Dictionary<int, SkillConfigData>();
        
        var loaded = CSVReader.LoadSkillCSV(fileName);
        if (loaded != null)
        {
            foreach (var kvp in loaded)
            {
                _skills[kvp.Key] = kvp.Value; // 覆蓋相同 ID
            }
        }
    }

    public static void Reload()
    {
        _skills = null;
    }

    public static SkillConfigData GetSkillConfig(int skillID)
    {
        if (Skills.TryGetValue(skillID, out var config))
        {
            return config;
        }
        return default;
    }
    
    // 依 Tag 取得技能列表 (例如 Shop,Megami)
    public static List<SkillConfigData> GetSkillsByTag(string tag)
    {
        var result = new List<SkillConfigData>();
        foreach (var skill in Skills.Values)
        {
            if (skill.tag == tag)
            {
                result.Add(skill);
            }
        }
        return result;
    }
}
