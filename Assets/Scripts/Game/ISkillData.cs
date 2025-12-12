using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
public struct SkillOrderData
{
    public string skillName;
    public SkillType skillType;
    public List<int> values;
    public bool isPlayerUse;
    public SkillOrderData(string name, SkillType type, List<int> val, bool isPlayer)
    {
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
    public int[] needDicesData;      // 需要的特定骰子點數組合
    public bool acceptMoreDice;      // 是否可以持續放入更多骰子

    // 技能需求配置
    public SkillRequirementType requirementType;  // 技能需求類型
    public int requiredSum;          // DiceSum 類型需要的骰子總和
    public int requiredSameCount;    // SameDices 類型需要的相同骰子數量
    public int requiredDiceCount;    // AnyDice 類型需要的骰子數量

    // 傷害計算配置
    public bool useDiceSumAsDamage;  // 是否使用骰子總和作為傷害值
    public float damageMultiplier;   // 傷害倍率

    // Buff 配置
    public BuffSeed[] selfBuffs;     // 對自己施加的 Buff 列表
    public BuffSeed[] targetBuffs;   // 對目標施加的 Buff 列表
}

public enum SkillType
{
    Attack,
    Heal,
    Buff
}
// 技能需求類型枚舉
public enum SkillRequirementType
{
    SpecificDices,    // 特定的骰子組合 (如 1,2,3)
    SameDices,        // 相同的骰子 (如 兩個相同)
    DiceSum,          // 骰子總和
    AnyDice,          // 任意骰子
    SpecificDicesWithRepeat // 指定骰子 但能重複(ex 單數骰*3之類的)
}
public interface ISkillData
{
    int skillID { get; set; }
    bool acceptMoreDice { get; set; } // 新增：是否可以持續放入骰子
    SkillType skillType { get; } // 新增：技能類型
    string skillName { get; set; }
    string conditionText { get; set; }// 技能條件描述
    string effectText { get; set; } // 技能效果描述
    int[] needDicesData { get; set; } // 需求骰子資料
    int skillValue { get; set; }
    List<BuffSeed> selfBuffs { get; set; }
    List<BuffSeed> targetBuffs { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();
    public void AddDiceData(int _dice);
    public void RemoveDiceData(int _dice);
    public List<int> GetNeedDices();
    public void Use(bool _isPlayer);
}
public class BaseSkill : ISkillData
{
    // 配置數據
    protected SkillConfigData config;

    public int skillID { get; set; } = 0;
    public bool acceptMoreDice { get; set; } = false;
    public string skillName { get; set; } = "";
    public SkillType skillType { get; set; } = SkillType.Attack;
    public string conditionText { get; set; } = "";
    public string effectText { get; set; } = "";
    public int skillValue { get; set; } = 0;
    public List<BuffSeed> selfBuffs { get; set; } = new List<BuffSeed>();
    public List<BuffSeed> targetBuffs { get; set; } = new List<BuffSeed>();
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDicesData { get; set; } = new int[] { };

    // 技能需求配置
    protected SkillRequirementType requirementType = SkillRequirementType.SpecificDices;
    protected int requiredSum = 0;        // 需要的總和
    protected int requiredSameCount = 0;  // 需要的相同數量
    protected int requiredDiceCount = 0;  // 需要的骰子數量

    // 傷害計算配置
    protected bool useDiceSumAsDamage = false;  // 是否使用骰子總和作為傷害
    protected float damageMultiplier = 1f;      // 傷害倍率

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
        skillName = configData.skillName;
        conditionText = configData.conditionText;
        effectText = configData.effectText;
        skillValue = configData.skillValue;
        needDicesData = configData.needDicesData ?? new int[] { };
        acceptMoreDice = configData.acceptMoreDice;
        skillType = configData.skillType;

        requirementType = configData.requirementType;
        requiredSum = configData.requiredSum;
        requiredSameCount = configData.requiredSameCount;
        requiredDiceCount = configData.requiredDiceCount;

        useDiceSumAsDamage = configData.useDiceSumAsDamage;
        damageMultiplier = configData.damageMultiplier;

        selfBuffs = configData.selfBuffs != null ? new List<BuffSeed>(configData.selfBuffs) : new List<BuffSeed>();
        targetBuffs = configData.targetBuffs != null ? new List<BuffSeed>(configData.targetBuffs) : new List<BuffSeed>();
    }

    public virtual bool canUseSkill()
    {
        return requirementType switch
        {
            SkillRequirementType.SpecificDices => needDicesData.All(n => diceBox.Contains(n)),
            SkillRequirementType.SameDices => diceBox.GroupBy(x => x).Any(g => g.Count() >= requiredSameCount),
            SkillRequirementType.DiceSum => diceBox.Sum() >= requiredSum,
            SkillRequirementType.AnyDice => diceBox.Count >= requiredDiceCount,
            SkillRequirementType.SpecificDicesWithRepeat => diceBox.Count(d => needDicesData.Contains(d)) >= requiredDiceCount,
            _ => false
        };
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
        if (!acceptMoreDice && canUseSkill())
        {
            return new List<int> { 666 };
        }

        // 根據需求類型返回對應的骰子需求
        return requirementType switch
        {
            SkillRequirementType.SpecificDices => GetSpecificDicesRequired(),
            SkillRequirementType.SameDices => GetSameDicesRequired(),
            SkillRequirementType.DiceSum => GetSumDicesRequired(),
            SkillRequirementType.AnyDice => new List<int> { 1, 2, 3, 4, 5, 6 },
            SkillRequirementType.SpecificDicesWithRepeat => GetSpecificDicesWithRepeatRequired(),
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
    // 使用技能
    public virtual void Use(bool _isPlayer)
    {
        if (canUseSkill())
        {
            // 計算傷害值
            int finalValue = useDiceSumAsDamage
                ? (int)(diceBox.Sum() * damageMultiplier)
                : skillValue;
            EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, skillName, skillType, new List<int> { finalValue }, _isPlayer);
            UnityEngine.Debug.Log($"{skillName} used, dealing {finalValue} skillValue!");
            if (selfBuffs.Count > 0)
            {
                foreach (var buff in selfBuffs)
                {
                    EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, "", SkillType.Buff, new List<int> { buff.buffID, buff.usageCount, buff.duration }, _isPlayer);
                }
            }
            if (targetBuffs.Count > 0)
            {
                foreach (var buff in targetBuffs)
                {
                    EventCenter.Dispatch(GameEvent.EVENT_USE_SKILL, "", SkillType.Buff, new List<int> { buff.buffID, buff.usageCount, buff.duration }, !_isPlayer);
                }
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