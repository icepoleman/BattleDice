using UnityEngine;
using System.Collections.Generic;
using System.IO;

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

    private string GetDialogueFolderPath()
    {
        return Path.Combine(Application.streamingAssetsPath, "Language", LanguageManager.CurrentLanguage, "Dialogue");
    }

    private string GetDialogueCsvPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string cleanFileName = fileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase)
            ? fileName
            : fileName + ".csv";

        return Path.Combine(GetDialogueFolderPath(), cleanFileName);
    }

    // 載入對話資料
    public List<DialogueData> LoadDialogueCSV(string fileName)
    {
        List<DialogueData> lines = new List<DialogueData>();

        string csvPath = GetDialogueCsvPath(fileName);
        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
        {
            Debug.LogError("❌ 找不到對話 CSV 檔案: " + csvPath);
            return null;
        }

        string csvText = File.ReadAllText(csvPath);
        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

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

        Debug.Log($"✅ 從 StreamingAssets 載入對話資料完成，共 {lines.Count} 行: {csvPath}");
        return lines;
    }

    // 取得當前語言下所有對話 CSV 名稱
    public List<string> GetAllDialogueCSVNames()
    {
        string folderPath = GetDialogueFolderPath();
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("❌ 找不到對話資料夾: " + folderPath);
            return new List<string>();
        }

        string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");
        List<string> names = new List<string>();

        foreach (string csvPath in csvFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(csvPath);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                names.Add(fileName);
            }
        }

        names.Sort();
        Debug.Log($"✅ 從 StreamingAssets/Language/{LanguageManager.CurrentLanguage}/Dialogue 取得對話 CSV 名稱，共 {names.Count} 個");
        return names;
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

        string streamingPath = Path.Combine(Application.streamingAssetsPath, "Game", "BuffData.csv");
        string csvText = File.Exists(streamingPath) ? File.ReadAllText(streamingPath) : null;

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("❌ 找不到 Buff CSV 檔案: " + streamingPath);
            return null;
        }

        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            // CSV 欄位: ID	Buff名稱	觸發時機	效果	數值	效果文
            BuffConfigData data = new BuffConfigData();
            data.buffID = values.Length > 0 && int.TryParse(values[0], out int id) ? id : 0;
            data.buffName = values.Length > 1 ? values[1] : "";
            data.buffTrigger = values.Length > 2 ? ParseBuffTrigger(values[2]) : BuffTrigger.OnApply;
            data.buffEffectType = values.Length > 3 ? ParseBuffEffectType(values[3]) : BuffEffectType.HP;
            data.effectValues = values.Length > 4 ? ParseIntArray(values[4]) : new int[] { };
            data.describe = values.Length > 5 ? values[5] : "";

            if (data.buffID > 0)
            {
                buffs[data.buffID] = data;
            }
        }

        Debug.Log($"✅ 從 StreamingAssets/Game 載入 Buff 資料完成，共 {buffs.Count} 筆: {streamingPath}");
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

        string cleanFileName = fileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ".csv";
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "Game", cleanFileName);
        string csvText = File.Exists(streamingPath) ? File.ReadAllText(streamingPath) : null;

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("❌ 找不到技能 CSV 檔案: " + streamingPath);
            return skills;
        }

        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            var data = SkillFactory.Create(
                skillID: v.Length > 0 && int.TryParse(v[0], out int skillId) ? skillId : 0,
                skillName: v.Length > 1 ? v[1] : "",
                skillType: v.Length > 2 ? ParseSkillType(v[2]) : SkillType.Attack,
                requirementType: v.Length > 3 ? ParseRequirementType(v[3]) : SkillRequirementType.SpecificDices,
                needDices: v.Length > 4 ? ParseIntArrayStatic(v[4]) : null,
                skillValue: v.Length > 5 && !string.IsNullOrWhiteSpace(v[5]) ? float.Parse(v[5]) : 0,
                conditionText: v.Length > 6 ? v[6] : "",
                effectText: v.Length > 7 ? v[7] : "",
                selfBuffs: v.Length > 8 ? ParseBuffSeeds(v[8]) : null,
                targetBuffs: v.Length > 9 ? ParseBuffSeeds(v[9]) : null,
                breakDiceCount: v.Length > 10 && !string.IsNullOrWhiteSpace(v[10]) ? int.Parse(v[10]) : 0,
                generateDices: v.Length > 11 ? ParseIntArrayStatic(v[11]) : null,
                tag: v.Length > 12 ? v[12] : "",
                price: v.Length > 13 && !string.IsNullOrWhiteSpace(v[13]) ? int.Parse(v[13]) : 0,
                iconPath: v.Length > 14 ? v[14] : ""
            );

            if (data.skillID > 0)
            {
                skills[data.skillID] = data;
            }
        }

        Debug.Log($"✅ 從 StreamingAssets/Game 載入技能資料完成，共 {skills.Count} 筆: {streamingPath}");
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

        string cleanFileName = fileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ".csv";
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "Game", cleanFileName);
        string csvText = File.Exists(streamingPath) ? File.ReadAllText(streamingPath) : null;

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("❌ 找不到敵人 CSV 檔案: " + streamingPath);
            return enemies;
        }

        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            var data = new EnemyConfigData
            {
                enemyId = v.Length > 0 && !string.IsNullOrWhiteSpace(v[0]) && int.TryParse(v[0], out int enemyId) ? enemyId : 0,
                enemyName = v.Length > 1 ? v[1] : "",
                goldReward = v.Length > 2 && !string.IsNullOrWhiteSpace(v[2]) && int.TryParse(v[2], out int gold) ? gold : 0,
                gearReward = v.Length > 3 && !string.IsNullOrWhiteSpace(v[3]) && int.TryParse(v[3], out int gear) ? gear : 0,
                maxBlood = v.Length > 4 && !string.IsNullOrWhiteSpace(v[4]) ? float.Parse(v[4]) : 100f,
                diceCount = v.Length > 5 && !string.IsNullOrWhiteSpace(v[5]) && int.TryParse(v[5], out int diceCount) ? diceCount : 2,
                skillIDs = v.Length > 6 ? ParseIntArrayStatic(v[6]) : new int[] { },
                initialBuffs = v.Length > 7 ? ParseBuffSeeds(v[7]) : null,
                enemyType = v.Length > 8 ? v[8] : "Zako",
                diceSides = v.Length > 9 ? ParseIntArrayStatic(v[9]) : new int[] { 1, 2, 3, 4, 5, 6 },
                imgId = v.Length > 10 ? v[10] : "",
                openStage = v.Length > 11 && !string.IsNullOrWhiteSpace(v[11]) && int.TryParse(v[11], out int openStage) ? openStage : 0,
            };

            if (data.enemyId > 0)
            {
                enemies[data.enemyId] = data;
            }
        }

        Debug.Log($"✅ 從 StreamingAssets/Game 載入敵人資料完成，共 {enemies.Count} 筆: {streamingPath}");
        return enemies;
    }

    //載入好感度故事資料
    // CSV 欄位: storyID, storyName, unlockHint
    public List<AffinityStoryData> LoadAffinityStoryCSV(string roleName)//根據角色名稱載入對應的好感度故事資料
    {
        List<AffinityStoryData> stories = new List<AffinityStoryData>();

        string csvPath = Path.Combine(GetDialogueFolderPath(), "AffinityStoryData.csv");
        if (!File.Exists(csvPath))
        {
            Debug.LogError("❌ 找不到好感度故事 CSV 檔案: " + csvPath);
            return stories;
        }

        string csvText = File.ReadAllText(csvPath);
        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            string storyID = v.Length > 0 ? v[0] : "";
            string storyName = v.Length > 1 ? v[1] : "";
            string unlockHint = v.Length > 2 ? v[2] : "";

            if (storyID == roleName)
            {
                stories.Add(new AffinityStoryData(storyID, storyName, unlockHint));
            }
        }

        Debug.Log($"✅ 從 StreamingAssets 載入好感度故事資料完成，共 {stories.Count} 筆: {csvPath}");
        return stories;
    }

    //載入整備室短劇情
    // CSV 欄位: role, dialogue, face
    public List<PreparationRoomShortData> LoadPreparationRoomShortCSV(string roleName)//根據角色名稱載入對應的整備室短劇情資料
    {
        List<PreparationRoomShortData> shortDataList = new List<PreparationRoomShortData>();

        string csvPath = Path.Combine(GetDialogueFolderPath(), "PreparationRoomShort.csv");
        if (!File.Exists(csvPath))
        {
            Debug.LogError("❌ 找不到整備室短劇情 CSV 檔案: " + csvPath);
            return shortDataList;
        }

        string csvText = File.ReadAllText(csvPath);
        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] v = line.Split(',');

            string role = v.Length > 0 ? v[0] : "";
            string dialogue = v.Length > 1 ? v[1] : "";
            string face = v.Length > 2 ? v[2] : "";

            if (role == roleName)
            {
                shortDataList.Add(new PreparationRoomShortData(role, dialogue, face));
            }
        }

        Debug.Log($"✅ 從 StreamingAssets 載入整備室短劇情資料完成，共 {shortDataList.Count} 筆: {csvPath}");
        return shortDataList;
    }

    // 通用 CSV 載入方法（可指定子資料夾）
    public List<T> LoadGenericCSV<T>(string fileName, string subFolder, System.Func<string[], T> parseFunction) where T : class
    {
        List<T> lines = new List<T>();

        string fileNameWithoutExt = fileName.EndsWith(".csv") ? fileName.Substring(0, fileName.Length - 4) : fileName;
        string csvPath = Path.Combine(Application.streamingAssetsPath, "Language", LanguageManager.CurrentLanguage, subFolder, fileNameWithoutExt + ".csv");

        if (!File.Exists(csvPath))
        {
            Debug.LogError($"❌ 找不到 CSV 檔案: {csvPath}");
            return null;
        }

        string csvText = File.ReadAllText(csvPath);
        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

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

        Debug.Log($"✅ 從 StreamingAssets/Language/{LanguageManager.CurrentLanguage}/{subFolder} 載入完成，共 {lines.Count} 行: {csvPath}");
        return lines;
    }
}
