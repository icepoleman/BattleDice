using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
public struct SkillOrderData
{
    public int skillID;
    public string skillName;
    public SkillType skillType;
    public List<int> values;
    public bool isPlayerUse;
    public SkillOrderData(int id, string name, SkillType type, List<int> val, bool isPlayer)
    {
        skillID = id;
        skillName = name;
        skillType = type;
        values = val;
        isPlayerUse = isPlayer;
    }
}

// 技能配置結構體
public struct SkillConfigData
{
    // 基本資訊
    public int skillID;              // 技能唯一識別碼
    public string skillName;         // 技能名稱
    public SkillType skillType;      // 技能類型 (攻擊/治療/Buff)
    public string conditionText;     // 技能條件描述文字
    public string effectText;        // 技能效果描述文字
    public int skillValue;           // 技能基礎數值 (傷害/治療量)
    public int[] needDicesData;      // 需要的骰子資料 (格式依 requirementType 不同)
    // SpecificDices: 特定骰子 (1,2,3)
    // SameDices: [0]=需要數量
    // DiceSum: [0]=需要總和
    // SpecificDicesWithRepeat: 允許骰子, 最後一個=需要數量
    // ConsecutiveDices: [0]=需要連續數量
    public int needDiceNum;          // 需要的骰子數量 (方便判斷)
    public int breakDiceCount;       // 玩家破壞骰子數量/怪物是否Combo
    public int[] generateDicesData;  // 生成骰子資料 允許骰子, 最後一個=需要數量
    public string tag;               // 標記技能出處
    // 技能需求配置
    public SkillRequirementType requirementType;  // 技能需求類型
    // Buff 配置
    public BuffSeed[] selfBuffs;     // 對自己施加的 Buff 列表
    public BuffSeed[] targetBuffs;   // 對目標施加的 Buff 列表
    public int price;                  // 技能價格（購買或升級用）
    public string iconPath;           // 技能圖示路徑

    /// <summary>
    /// 計算需要的骰子數量 (根據 requirementType 和 needDicesData)
    /// </summary>
    public int GetNeedDiceNum()
    {
        if (needDiceNum > 0) return needDiceNum; // 如果已設定則直接返回
        if (needDicesData == null || needDicesData.Length == 0) return 0;

        return requirementType switch
        {
            SkillRequirementType.SpecificDices => needDicesData.Length,
            SkillRequirementType.SameDices => needDicesData[0],
            SkillRequirementType.SpecificDicesWithRepeat => needDicesData[needDicesData.Length - 1],
            SkillRequirementType.ConsecutiveDices => needDicesData[0],
            SkillRequirementType.AnyDices => needDicesData[0],
            SkillRequirementType.DiceSum => 0, // DiceSum 無法確定數量
            _ => 0
        };
    }
}

public enum SkillType
{
    Attack,
    Heal,
    Buff,
    DeBuff
}
// 技能需求類型枚舉
public enum SkillRequirementType
{
    SpecificDices,    // 特定的骰子組合 (如 1,2,3)
    SameDices,        // 任意相同的骰子 (如 兩個相同)
    DiceSum,          // 骰子總和 (如 總和 >= 10)
    SpecificDicesWithRepeat, // 指定骰子 但能重複(ex 單數骰*3之類的)
    ConsecutiveDices, // 連續骰子 (如 123, 234, 345 等)
    AnyDices          // 任意骰子達到數量即可 (如 任意3顆)
}
public interface ISkillData
{
    int skillID { get; set; }
    string iconPath { get; set; } // 新增：技能圖示路徑
    SkillType skillType { get; } // 新增：技能類型
    string skillName { get; set; }
    string conditionText { get; set; }// 技能條件描述
    string effectText { get; set; } // 技能效果描述
    int[] needDicesData { get; set; } // 需求骰子資料
    int needDiceNum { get; } // 需要的骰子數量
    int breakDiceCount { get; set; } // 破壞骰子數量
    int skillValue { get; set; }
    List<BuffSeed> selfBuffs { get; set; }
    List<BuffSeed> targetBuffs { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();
    public void AddDiceData(int _dice);
    public void RemoveDiceData(int _dice);
    public List<int> GetNeedDices();
    public void UseSkill(bool _isPlayer);

    // 用於怪物多技能判斷：給定可用骰子，判斷能否發動
    public bool CanUseWithDice(List<int> availableDice);
    // 取得此技能會消耗的骰子（從給定的可用骰子中）
    public List<int> GetUsedDices(List<int> availableDice);
}
public class BaseSkill : ISkillData
{
    // 配置數據
    protected SkillConfigData config;

    // 萬用骰（特殊骰）
    public const int WILD_DICE = 0;

    public int skillID { get; set; } = 0;
    public string skillName { get; set; } = "";
    public SkillType skillType { get; set; } = SkillType.Attack;
    public string iconPath { get; set; } = "";
    public string conditionText { get; set; } = "";
    public string effectText { get; set; } = "";
    public int skillValue { get; set; } = 0;
    public int breakDiceCount { get; set; } = 0;
    public int[] generateDicesData { get; set; } = new int[] { };
    public List<BuffSeed> selfBuffs { get; set; } = new List<BuffSeed>();
    public List<BuffSeed> targetBuffs { get; set; } = new List<BuffSeed>();
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDicesData { get; set; } = new int[] { };

    // 需要的骰子數量
    public int needDiceNum => GetNeedDiceNumInternal();

    // 技能需求配置
    protected SkillRequirementType requirementType = SkillRequirementType.SpecificDices;

    // 計算需要的骰子數量
    private int GetNeedDiceNumInternal()
    {
        if (needDicesData == null || needDicesData.Length == 0) return 0;

        return requirementType switch
        {
            SkillRequirementType.SpecificDices => needDicesData.Length,
            SkillRequirementType.SameDices => needDicesData[0],
            SkillRequirementType.SpecificDicesWithRepeat => needDicesData[needDicesData.Length - 1],
            SkillRequirementType.ConsecutiveDices => needDicesData[0],
            SkillRequirementType.AnyDices => needDicesData[0],
            SkillRequirementType.DiceSum => 0,
            _ => 0
        };
    }

    // 預設建構子
    public BaseSkill() { }

    // 使用配置數據建構
    public BaseSkill(int skillID)
    {
        // 從資料庫載入配置
        var configData = SkillDatabase.GetSkillConfig(skillID);
        ApplyConfig(configData);
    }

    // 套用配置
    protected void ApplyConfig(SkillConfigData configData)
    {
        config = configData;
        skillID = configData.skillID;
        iconPath = configData.iconPath;
        skillName = configData.skillName;
        conditionText = configData.conditionText;
        effectText = configData.effectText;
        skillValue = configData.skillValue;
        needDicesData = configData.needDicesData ?? new int[] { };
        breakDiceCount = configData.breakDiceCount;
        skillType = configData.skillType;
        generateDicesData = configData.generateDicesData ?? new int[] { };

        requirementType = configData.requirementType;

        selfBuffs = configData.selfBuffs != null ? new List<BuffSeed>(configData.selfBuffs) : new List<BuffSeed>();
        targetBuffs = configData.targetBuffs != null ? new List<BuffSeed>(configData.targetBuffs) : new List<BuffSeed>();
    }

    public virtual bool canUseSkill()
    {
        return requirementType switch
        {
            SkillRequirementType.SpecificDices => CheckSpecificDicesWithWild(diceBox, needDicesData),
            SkillRequirementType.SameDices => CheckSameDicesWithWild(diceBox, GetNeedCount()),
            SkillRequirementType.DiceSum => diceBox.Sum() >= GetRequiredSum(),
            SkillRequirementType.SpecificDicesWithRepeat => CheckSpecificDicesWithRepeatAndWild(diceBox, GetAllowedDices(), GetNeedCount()),
            SkillRequirementType.ConsecutiveDices => CheckConsecutiveDicesWithWild(diceBox, GetNeedCount()),
            SkillRequirementType.AnyDices => diceBox.Count >= GetNeedCount(),
            _ => false
        };
    }

    // 獲取需要的數量 (SameDices/ConsecutiveDices: [0], SpecificDicesWithRepeat: 最後一個)
    protected int GetNeedCount()
    {
        if (needDicesData == null || needDicesData.Length == 0) return 0;

        return requirementType switch
        {
            SkillRequirementType.SpecificDicesWithRepeat => needDicesData[needDicesData.Length - 1],
            _ => needDicesData[0]
        };
    }

    // 獲取 SpecificDicesWithRepeat 允許的骰子 (除了最後一個數字)
    protected int[] GetAllowedDices()
    {
        if (needDicesData == null || needDicesData.Length <= 1) return new int[] { };
        return needDicesData.Take(needDicesData.Length - 1).ToArray();
    }

    // 獲取 DiceSum 所需的總和 (needDicesData[0])
    protected int GetRequiredSum()
    {
        return needDicesData != null && needDicesData.Length > 0 ? needDicesData[0] : 0;
    }

    // 獲取生成骰子的數量 (generateDicesData 最後一個數字)
    protected int GetGenerateCount()
    {
        if (generateDicesData == null || generateDicesData.Length == 0) return 0;
        return generateDicesData[generateDicesData.Length - 1];
    }

    // 獲取生成骰子允許的點數 (generateDicesData 除了最後一個數字)
    // 0 = 萬用骰
    protected int[] GetGenerateAllowedDices()
    {
        if (generateDicesData == null || generateDicesData.Length <= 1) return new int[] { };
        return generateDicesData.Take(generateDicesData.Length - 1).ToArray();
    }

    public void AddDiceData(int _dice)
    {
        diceBox.Add(_dice);
    }

    public void RemoveDiceData(int _dice)
    {
        diceBox.Remove(_dice);
    }

    public virtual List<int> GetNeedDices()
    {
        // 達成條件且不接受更多骰子時返回無效值
        /*if (!acceptMoreDice && canUseSkill())
        {
            return new List<int> { 666 };
        }*/

        // 根據需求類型返回對應的骰子需求
        return requirementType switch
        {
            SkillRequirementType.SpecificDices => GetSpecificDicesRequired(),
            SkillRequirementType.SameDices => GetSameDicesRequired(),
            SkillRequirementType.DiceSum => GetSumDicesRequired(),
            SkillRequirementType.SpecificDicesWithRepeat => GetSpecificDicesWithRepeatRequired(),
            SkillRequirementType.ConsecutiveDices => GetConsecutiveDicesRequired(),
            SkillRequirementType.AnyDices => GetAnyDicesRequired(),
            _ => new List<int>()
        };
    }
    // 獲取特定骰子需求
    protected virtual List<int> GetSpecificDicesRequired()
    {
        List<int> needDices = new List<int>(needDicesData);
        needDices.RemoveAll(n => diceBox.Contains(n));
        return needDices;
    }
    // 獲取相同骰子需求
    protected virtual List<int> GetSameDicesRequired()
    {
        if (diceBox.Count > 0)
        {
            return new List<int> { diceBox[0] };
        }
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }
    // 獲取骰子總和需求
    protected virtual List<int> GetSumDicesRequired()
    {
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }
    // 獲取特定骰子可重複需求
    protected virtual List<int> GetSpecificDicesWithRepeatRequired()
    {
        List<int> needDices = new List<int>(needDicesData);
        return needDices;
    }

    // 獲取連續骰子需求
    protected virtual List<int> GetConsecutiveDicesRequired()
    {
        // 返回所有可能的骰子，讓玩家自己組合
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }

    // 獲取任意骰子需求
    protected virtual List<int> GetAnyDicesRequired()
    {
        // 返回所有可能的骰子
        return new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    }

    // 檢查是否有連續骰子
    private bool CheckConsecutiveDices(List<int> dices)
    {
        int count = GetNeedCount();
        if (dices.Count < count) return false;

        // 取得不重複且排序的骰子
        var sortedUnique = dices.Distinct().OrderBy(x => x).ToList();

        // 嘗試找到連續 count 個的序列
        for (int i = 0; i <= sortedUnique.Count - count; i++)
        {
            bool isConsecutive = true;
            for (int j = 0; j < count - 1; j++)
            {
                if (sortedUnique[i + j + 1] - sortedUnique[i + j] != 1)
                {
                    isConsecutive = false;
                    break;
                }
            }
            if (isConsecutive) return true;
        }
        return false;
    }

    // 取得連續骰子組合（用於 GetUsedDices）
    private List<int> GetConsecutiveSequence(List<int> dices)
    {
        int count = GetNeedCount();
        if (dices.Count < count) return new List<int>();

        var sortedUnique = dices.Distinct().OrderBy(x => x).ToList();

        for (int i = 0; i <= sortedUnique.Count - count; i++)
        {
            bool isConsecutive = true;
            for (int j = 0; j < count - 1; j++)
            {
                if (sortedUnique[i + j + 1] - sortedUnique[i + j] != 1)
                {
                    isConsecutive = false;
                    break;
                }
            }
            if (isConsecutive)
            {
                // 返回這個連續序列
                return sortedUnique.Skip(i).Take(count).ToList();
            }
        }
        return new List<int>();
    }

    // 用於怪物多技能判斷：給定可用骰子，判斷能否發動
    public virtual bool CanUseWithDice(List<int> availableDice)
    {
        return requirementType switch
        {
            SkillRequirementType.SpecificDices => CheckSpecificDicesWithWild(availableDice, needDicesData),
            SkillRequirementType.SameDices => CheckSameDicesWithWild(availableDice, GetNeedCount()),
            SkillRequirementType.DiceSum => availableDice.Sum() >= GetRequiredSum(),
            SkillRequirementType.SpecificDicesWithRepeat => CheckSpecificDicesWithRepeatAndWild(availableDice, GetAllowedDices(), GetNeedCount()),
            SkillRequirementType.ConsecutiveDices => CheckConsecutiveDicesWithWild(availableDice, GetNeedCount()),
            SkillRequirementType.AnyDices => availableDice.Count >= GetNeedCount(),
            _ => false
        };
    }

    // ===== 萬用骰處理方法 =====

    // 檢查特定骰子（支援萬用骰）
    private bool CheckSpecificDicesWithWild(List<int> dices, int[] required)
    {
        List<int> tempDice = new List<int>(dices);
        int wildCount = tempDice.Count(d => d == WILD_DICE);
        tempDice.RemoveAll(d => d == WILD_DICE);

        foreach (int need in required)
        {
            if (tempDice.Contains(need))
            {
                tempDice.Remove(need);
            }
            else if (wildCount > 0)
            {
                wildCount--;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    // 檢查相同骰子（支援萬用骰）
    private bool CheckSameDicesWithWild(List<int> dices, int requiredCount)
    {
        int wildCount = dices.Count(d => d == WILD_DICE);
        var normalDice = dices.Where(d => d != WILD_DICE).ToList();

        // 如果全部都是萬用骰
        if (normalDice.Count == 0)
        {
            return wildCount >= requiredCount;
        }

        // 找每種骰子的數量 + 萬用骰
        var groups = normalDice.GroupBy(x => x);
        foreach (var group in groups)
        {
            if (group.Count() + wildCount >= requiredCount)
            {
                return true;
            }
        }
        return false;
    }

    // 檢查指定骰子可重複（支援萬用骰）
    private bool CheckSpecificDicesWithRepeatAndWild(List<int> dices, int[] allowedDices, int requiredCount)
    {
        int wildCount = dices.Count(d => d == WILD_DICE);
        int matchCount = dices.Count(d => allowedDices.Contains(d));
        return matchCount + wildCount >= requiredCount;
    }

    // 檢查連續骰子（支援萬用骰）
    private bool CheckConsecutiveDicesWithWild(List<int> dices, int requiredCount)
    {
        if (dices.Count < requiredCount) return false;

        int wildCount = dices.Count(d => d == WILD_DICE);
        var normalDice = dices.Where(d => d != WILD_DICE).Distinct().OrderBy(x => x).ToList();

        // 如果全是萬用骰，只要數量夠就行
        if (normalDice.Count == 0)
        {
            return wildCount >= requiredCount;
        }

        // 嘗試所有可能的起始點 (1-6)
        for (int start = 1; start <= 7 - requiredCount; start++)
        {
            int neededWilds = 0;

            for (int i = 0; i < requiredCount; i++)
            {
                int target = start + i;
                if (!normalDice.Contains(target))
                {
                    neededWilds++;
                }
            }

            if (neededWilds <= wildCount)
            {
                return true;
            }
        }
        return false;
    }

    // 取得此技能會消耗的骰子（從給定的可用骰子中）
    public virtual List<int> GetUsedDices(List<int> availableDice)
    {
        List<int> usedDices = new List<int>();
        List<int> tempDice = new List<int>(availableDice);

        switch (requirementType)
        {
            case SkillRequirementType.SpecificDices:
                // 消耗指定的骰子（支援萬用骰）
                foreach (int need in needDicesData)
                {
                    if (tempDice.Contains(need))
                    {
                        usedDices.Add(need);
                        tempDice.Remove(need);
                    }
                    else if (tempDice.Contains(WILD_DICE))
                    {
                        usedDices.Add(WILD_DICE);
                        tempDice.Remove(WILD_DICE);
                    }
                }
                break;

            case SkillRequirementType.SameDices:
                // 找到數量最多的相同骰子組（支援萬用骰）
                int wildCount = tempDice.Count(d => d == WILD_DICE);
                var normalDice = tempDice.Where(d => d != WILD_DICE).ToList();
                int needCount = GetNeedCount();

                if (normalDice.Count > 0)
                {
                    var bestGroup = normalDice.GroupBy(x => x)
                        .OrderByDescending(g => g.Count())
                        .First();

                    int takeFromGroup = Math.Min(bestGroup.Count(), needCount);
                    usedDices.AddRange(bestGroup.Take(takeFromGroup));

                    // 用萬用骰補足
                    int remaining = needCount - takeFromGroup;
                    for (int i = 0; i < remaining && wildCount > 0; i++)
                    {
                        usedDices.Add(WILD_DICE);
                        wildCount--;
                    }
                }
                else
                {
                    // 全是萬用骰
                    for (int i = 0; i < needCount && wildCount > 0; i++)
                    {
                        usedDices.Add(WILD_DICE);
                        wildCount--;
                    }
                }
                break;

            case SkillRequirementType.DiceSum:
                // 總和類型：消耗所有骰子
                usedDices.AddRange(tempDice);
                break;

            case SkillRequirementType.SpecificDicesWithRepeat:
                // 可重複指定骰子（支援萬用骰）
                int count = 0;
                int repeatNeedCount = GetNeedCount();
                int[] allowedDices = GetAllowedDices();
                // 先用符合的骰子
                foreach (int dice in tempDice.ToList())
                {
                    if (allowedDices.Contains(dice) && count < repeatNeedCount)
                    {
                        usedDices.Add(dice);
                        tempDice.Remove(dice);
                        count++;
                    }
                }
                // 再用萬用骰補足
                foreach (int dice in tempDice.ToList())
                {
                    if (dice == WILD_DICE && count < repeatNeedCount)
                    {
                        usedDices.Add(dice);
                        count++;
                    }
                }
                break;

            case SkillRequirementType.ConsecutiveDices:
                // 連續骰子（支援萬用骰）
                var consecutive = GetConsecutiveSequenceWithWild(tempDice);
                usedDices.AddRange(consecutive);
                break;

            case SkillRequirementType.AnyDices:
                // 任意骰子：取需要的數量
                int anyNeedCount = GetNeedCount();
                for (int i = 0; i < anyNeedCount && tempDice.Count > 0; i++)
                {
                    usedDices.Add(tempDice[0]);
                    tempDice.RemoveAt(0);
                }
                break;
        }

        return usedDices;
    }

    // 取得連續骰子組合（支援萬用骰）
    private List<int> GetConsecutiveSequenceWithWild(List<int> dices)
    {
        int count = GetNeedCount();
        if (dices.Count < count) return new List<int>();

        int wildCount = dices.Count(d => d == WILD_DICE);
        var normalDice = dices.Where(d => d != WILD_DICE).Distinct().OrderBy(x => x).ToList();

        // 嘗試所有可能的起始點
        for (int start = 1; start <= 7 - count; start++)
        {
            List<int> result = new List<int>();
            int wildsUsed = 0;
            bool possible = true;

            for (int i = 0; i < count; i++)
            {
                int target = start + i;
                if (normalDice.Contains(target))
                {
                    result.Add(target);
                }
                else if (wildsUsed < wildCount)
                {
                    result.Add(WILD_DICE);
                    wildsUsed++;
                }
                else
                {
                    possible = false;
                    break;
                }
            }

            if (possible)
            {
                return result;
            }
        }
        return new List<int>();
    }

    // 使用技能
    public virtual void UseSkill(bool _isPlayer)
    {
        if (canUseSkill())
        {
            // 計算傷害值
            int finalValue = skillValue;
            EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, skillID, skillName, skillType, new List<int> { finalValue }, _isPlayer);
            UnityEngine.Debug.Log($"{skillName} used, dealing {finalValue} skillValue!");
            if (selfBuffs.Count > 0)
            {
                foreach (var buff in selfBuffs)
                {
                    BaseBuff spBuff = new BaseBuff(buff.buffID, buff.usageCount, buff.duration);
                    // 生成新的 Buff 並套用到角色
                    EventCenter.Dispatch(GameEvent.EVENT_ADD_BUFF, spBuff, _isPlayer);
                    //EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, skillID, "", SkillType.Buff, new List<int> { buff.buffID, buff.usageCount, buff.duration }, _isPlayer);
                }
            }
            if (targetBuffs.Count > 0)
            {
                foreach (var buff in targetBuffs)
                {
                    BaseBuff spBuff = new BaseBuff(buff.buffID, buff.usageCount, buff.duration);
                    // 生成新的 Buff 並套用到角色
                    EventCenter.Dispatch(GameEvent.EVENT_ADD_BUFF, spBuff, !_isPlayer);
                    //EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, skillID, "", SkillType.Buff, new List<int> { buff.buffID, buff.usageCount, buff.duration }, !_isPlayer);
                }
            }
            if (breakDiceCount > 0)
            {
                if (_isPlayer)
                {
                    // 玩家：破壞敵人骰子
                    UnityEngine.Debug.Log($"{skillName} will destroy {breakDiceCount} enemy dice!");
                    EventCenter.Dispatch(GameEvent.EVENT_DESTROY_ENEMY_DICE, breakDiceCount);
                }
                else
                {
                    // 怪物：觸發重骰再攻擊
                    UnityEngine.Debug.Log($"{skillName} triggers enemy reroll!");
                    EventCenter.Dispatch(GameEvent.EVENT_ENEMY_REROLL);
                }
            }
            if (generateDicesData != null && generateDicesData.Length > 0)
            {
                int generateCount = GetGenerateCount();
                int[] allowedDices = GetGenerateAllowedDices();
                UnityEngine.Debug.Log($"{skillName} will generate {generateCount} dice from allowed: {string.Join(",", allowedDices)}");
                EventCenter.Dispatch(GameEvent.EVENT_GENERATE_MANA_DICE, allowedDices, generateCount);
            }
        }
        else
        {
            UnityEngine.Debug.Log($"{skillName} cannot be used, insufficient dice!");
        }
        //使用技能後清空骰子
        diceBox.Clear();
    }
}