using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueData
{
    public string Chapter;
    public string Character;
    public string Dialogue;//對話內容
    public string Portrait;//表情立繪
    public string Pos;
    public string[] Choices;
    public string[] JumpTo;
    public string Tag;
    public string Anim;
    public string Flag;
    public string Background;
    public string CameraAnim;
    public string Sound;
}
[System.Serializable]
public class MapData
{
    public string stageID;
    public string type;
    public string stageInfo;
}

public class CSVReader
{
    //單例模式
    private static CSVReader instance;
    public static CSVReader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CSVReader();
            }
            return instance;
        }
    }

    // 載入對話資料
    public List<DialogueData> LoadDialogueCSV(string fileName)
    {
        List<DialogueData> lines = new List<DialogueData>();

        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/Dialogue/{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError("❌ 找不到對話 CSV 檔案: " + resourcePath);
            return null;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            DialogueData data = new DialogueData();
            data.Chapter = values.Length > 0 ? values[0] : "";
            data.Character = values.Length > 1 ? values[1] : "";
            data.Dialogue = values.Length > 2 ? values[2] : "";
            data.Portrait = values.Length > 3 ? values[3] : "";
            data.Pos = values.Length > 4 ? values[4] : "";

            data.Choices = values.Length > 5 && !string.IsNullOrEmpty(values[5]) ? values[5].Split('|') : new string[0];
            data.JumpTo = values.Length > 6 && !string.IsNullOrEmpty(values[6]) ? values[6].Split('|') : new string[0];

            data.CameraAnim = values.Length > 7 ? values[7] : "";
            data.Anim = values.Length > 8 ? values[8] : "";
            data.Flag = values.Length > 9 ? values[9] : "";
            data.Background = values.Length > 10 ? values[10] : "";
            data.Tag = values.Length > 11 ? values[11] : "";
            data.Sound = values.Length > 12 ? values[12] : "";
            lines.Add(data);
        }

        Debug.Log($"✅ 從 Resources 載入對話資料完成，共 {lines.Count} 行: {resourcePath}");
        return lines;
    }

    // 載入地圖資料
    public List<MapData> LoadMapCSV(string fileName)
    {
        List<MapData> lines = new List<MapData>();

        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/Map/{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError("❌ 找不到地圖 CSV 檔案: " + resourcePath);
            return null;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            MapData data = new MapData();
            data.stageID = values.Length > 0 ? values[0] : "";
            data.type = values.Length > 1 ? values[1] : "";
            data.stageInfo = values.Length > 2 ? values[2] : "";

            lines.Add(data);
        }

        Debug.Log($"✅ 從 Resources 載入地圖資料完成，共 {lines.Count} 行: {resourcePath}");
        return lines;
    }
    //載入BUFF資料
    public Dictionary<int, BuffConfigData> LoadBuffCSV()
    {
        Dictionary<int, BuffConfigData> buffs = new Dictionary<int, BuffConfigData>();

        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/Game/BuffData";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError("❌ 找不到 Buff CSV 檔案: " + resourcePath);
            return null;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            // CSV 欄位: ID	Buff名稱	觸發時機	效果	數值	效果文
            BuffConfigData data = new BuffConfigData();
            data.buffID = values.Length > 0 ? int.Parse(values[0]) : 0;
            data.buffName = values.Length > 1 ? values[1] : "";
            data.buffTrigger = values.Length > 2 ? ParseBuffTrigger(values[2]) : BuffTrigger.OnApply;
            data.buffEffectType = values.Length > 3 ? ParseBuffEffectType(values[3]) : BuffEffectType.HP;
            data.effectValues = values.Length > 4 ? ParseIntArray(values[4]) : new int[] { };
            data.describe = values.Length > 5 ? values[5] : "";

            buffs[data.buffID] = data;
        }

        Debug.Log($"✅ 從 Resources 載入 Buff 資料完成，共 {buffs.Count} 筆: {resourcePath}");
        return buffs;
    }

    // 解析 BuffTrigger 字串
    private BuffTrigger ParseBuffTrigger(string value)
    {
        if (System.Enum.TryParse<BuffTrigger>(value.Trim(), true, out var result))
            return result;
        return BuffTrigger.OnApply;
    }

    // 解析 BuffEffectType 字串
    private BuffEffectType ParseBuffEffectType(string value)
    {
        if (System.Enum.TryParse<BuffEffectType>(value.Trim(), true, out var result))
            return result;
        return BuffEffectType.HP;
    }

    // 解析整數陣列 (用 | 分隔)
    private int[] ParseIntArray(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new int[] { };

        string[] parts = value.Split('|');
        int[] result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            int.TryParse(parts[i].Trim(), out result[i]);
        }
        return result;
    }

    // 載入技能資料
    // CSV 欄位: skillID, skillName, skillType, requirementType, needDices, skillValue, conditionText, effectText, selfBuffs, targetBuffs, breakDiceCount, generateDices, tag,price
    // needDices 格式 (用 | 分隔):
    //   SpecificDices: 特定骰子 (1|2|3)
    //   SameDices: 需要數量 (2)
    //   DiceSum: 需要總和 (5)
    //   SpecificDicesWithRepeat: 允許骰子+數量 (1|3|5|3) 最後一個是數量
    //   ConsecutiveDices: 需要連續數量 (3)
    // generateDices 格式: 允許骰子+數量 (1|3|5|2) = 從1,3,5中隨機生成2個，0=萬用骰
    public static Dictionary<int, SkillConfigData> LoadSkillCSV(string fileName)
    {
        Dictionary<int, SkillConfigData> skills = new Dictionary<int, SkillConfigData>();

        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/Game/{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError("❌ 找不到技能 CSV 檔案: " + resourcePath);
            return skills;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            var data = SkillFactory.Create(
                skillID: v.Length > 0 ? int.Parse(v[0]) : 0,
                skillName: v.Length > 1 ? v[1] : "",
                skillType: v.Length > 2 ? ParseSkillType(v[2]) : SkillType.Attack,
                requirementType: v.Length > 3 ? ParseRequirementType(v[3]) : SkillRequirementType.SpecificDices,
                needDices: v.Length > 4 ? ParseIntArrayStatic(v[4]) : null,
                skillValue: v.Length > 5 && !string.IsNullOrWhiteSpace(v[5]) ? int.Parse(v[5]) : 0,
                conditionText: v.Length > 6 ? v[6] : "",
                effectText: v.Length > 7 ? v[7] : "",
                selfBuffs: v.Length > 8 ? ParseBuffSeeds(v[8]) : null,
                targetBuffs: v.Length > 9 ? ParseBuffSeeds(v[9]) : null,
                breakDiceCount: v.Length > 10 && !string.IsNullOrWhiteSpace(v[10]) ? int.Parse(v[10]) : 0,
                generateDices: v.Length > 11 ? ParseIntArrayStatic(v[11]) : null,
                tag: v.Length > 12 ? v[12] : "",
                price: v.Length > 13 && !string.IsNullOrWhiteSpace(v[13]) ? int.Parse(v[13]) : 0
            );

            skills[data.skillID] = data;
        }

        Debug.Log($"✅ 從 Resources 載入技能資料完成，共 {skills.Count} 筆: {resourcePath}");
        return skills;
    }

    // 解析 SkillType
    private static SkillType ParseSkillType(string value)
    {
        if (System.Enum.TryParse<SkillType>(value.Trim(), true, out var result))
            return result;
        return SkillType.Attack;
    }

    // 解析 SkillRequirementType
    private static SkillRequirementType ParseRequirementType(string value)
    {
        if (System.Enum.TryParse<SkillRequirementType>(value.Trim(), true, out var result))
            return result;
        return SkillRequirementType.SpecificDices;
    }

    // 解析整數陣列 (用 | 分隔) - 靜態版本
    private static int[] ParseIntArrayStatic(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new int[] { };

        string[] parts = value.Split('|');
        int[] result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            int.TryParse(parts[i].Trim(), out result[i]);
        }
        return result;
    }

    // 解析 BuffSeed 陣列 (格式: buffID:value:duration | buffID:value:duration)
    private static BuffSeed[] ParseBuffSeeds(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string[] parts = value.Split('|');
        List<BuffSeed> seeds = new List<BuffSeed>();

        foreach (string part in parts)
        {
            string[] buffParts = part.Split(':');
            if (buffParts.Length >= 1)
            {
                int buffID = int.Parse(buffParts[0].Trim());
                int buffValue = buffParts.Length > 1 ? int.Parse(buffParts[1].Trim()) : 0;
                int duration = buffParts.Length > 2 ? int.Parse(buffParts[2].Trim()) : 0;
                seeds.Add(new BuffSeed(buffID, buffValue, duration));
            }
        }

        return seeds.Count > 0 ? seeds.ToArray() : null;
    }

    // 載入敵人資料
    // CSV 欄位: enemyId, enemyName, goldReward, gearReward, maxBlood, diceCount, skillIDs, initialBuffs
    // skillIDs 格式: 101|102|103
    // initialBuffs 格式: buffID:usageCount:duration | buffID:usageCount:duration
    public static Dictionary<int, EnemyConfigData> LoadEnemyCSV(string fileName)
    {
        Dictionary<int, EnemyConfigData> enemies = new Dictionary<int, EnemyConfigData>();

        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/Game/{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError("❌ 找不到敵人 CSV 檔案: " + resourcePath);
            return enemies;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            var data = new EnemyConfigData
            {
                enemyId = v.Length > 0 && !string.IsNullOrWhiteSpace(v[0]) ? int.Parse(v[0]) : 0,
                enemyName = v.Length > 1 ? v[1] : "",
                goldReward = v.Length > 2 && !string.IsNullOrWhiteSpace(v[2]) ? int.Parse(v[2]) : 0,
                gearReward = v.Length > 3 && !string.IsNullOrWhiteSpace(v[3]) ? int.Parse(v[3]) : 0,
                maxBlood = v.Length > 4 && !string.IsNullOrWhiteSpace(v[4]) ? float.Parse(v[4]) : 100f,
                diceCount = v.Length > 5 && !string.IsNullOrWhiteSpace(v[5]) ? int.Parse(v[5]) : 2,
                skillIDs = v.Length > 6 ? ParseIntArrayStatic(v[6]) : new int[] { },
                initialBuffs = v.Length > 7 ? ParseBuffSeeds(v[7]) : null
            };

            if (data.enemyId > 0)
            {
                enemies[data.enemyId] = data;
            }
        }

        Debug.Log($"✅ 從 Resources 載入敵人資料完成，共 {enemies.Count} 筆: {resourcePath}");
        return enemies;
    }

    // 通用 CSV 載入方法（可指定子資料夾）
    public List<T> LoadGenericCSV<T>(string fileName, string subFolder, System.Func<string[], T> parseFunction) where T : class
    {
        List<T> lines = new List<T>();

        // 移除 .csv 副檔名（Resources.Load 不需要）
        string fileNameWithoutExt = fileName.EndsWith(".csv") ? fileName.Substring(0, fileName.Length - 4) : fileName;
        string resourcePath = $"Language/{LanguageManager.CurrentLanguage}/{subFolder}/{fileNameWithoutExt}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"❌ 找不到 CSV 檔案: {resourcePath}");
            return null;
        }

        string[] allLines = textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            T data = parseFunction(values);

            if (data != null)
                lines.Add(data);
        }

        Debug.Log($"✅ 從 Resources/{subFolder} 載入完成，共 {lines.Count} 行: {resourcePath}");
        return lines;
    }
}
