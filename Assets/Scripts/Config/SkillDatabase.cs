// 技能配置資料庫 - 從 CSV 載入
using System.Collections.Generic;

public static class SkillDatabase
{
    public static HashSet<int> SpSkillIDs = new HashSet<int> {38, 39}; // 特殊技能ID列表，供遊戲邏輯判斷使用
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

        // 統一處理本地化文本
        SkillLocalizationHelper.LocalizeAllSkills(_skills); 
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
