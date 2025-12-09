using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// 技能需求類型枚舉
public enum SkillRequirementType
{
    SpecificDices,    // 特定的骰子組合 (如 1,2,3)
    SameDices,        // 相同的骰子 (如 兩個相同)
    DiceSum,          // 骰子總和
    AnyDice           // 任意骰子
}

// 技能工廠
public static class SkillFactory
{
    public static ISkillData CreateSkill(int skillId)
    {
        switch (skillId)
        {
            case 1:
                return new FireBall();
            case 2:
                return new Kaminari();
            case 3:
                return new WindBlade();
            // case (int)SkillID.DeBuffPoison:
            //     return new DeBuffPoison();
            case 4:
                return new Punch();
            case 5:
                return new ClawAttack();
            default:
                UnityEngine.Debug.LogError("SkillFactory: 未知的技能 ID " + skillId);
                return null;
        }
    }
}
public interface ISkillData
{
    int skillID { get; set; }
    bool isDebuff { get; set; }
    bool acceptMoreDice { get; set; } // 新增：是否可以持續放入骰子
    string skillName { get; set; }
    string conditionText { get; set; }// 技能條件描述
    string effectText { get; set; } // 技能效果描述
    int[] needDicesData { get; set; } // 需求骰子資料
    float damage { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();
    public void AddDiceData(int _dice);
    public void RemoveDiceData(int _dice);
    public List<int> GetNeedDices();
    public void Use(bool _isPlayer);
}
public class BaseSkill : ISkillData
{
    public int skillID { get; set; } = 0;
    public bool isDebuff { get; set; } = false;
    public bool acceptMoreDice { get; set; } = false;
    public string skillName { get; set; } = "BaseSkill";
    public string conditionText { get; set; } = "";
    public string effectText { get; set; } = "";
    public float damage { get; set; } = 0f;
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDicesData { get; set; } = new int[] { };
    
    // 技能需求配置
    protected SkillRequirementType requirementType = SkillRequirementType.SpecificDices;
    protected int requiredSum = 0;        // 需要的總和
    protected int requiredSameCount = 2;  // 需要的相同數量
    
    public virtual bool canUseSkill()
    {
        return false;
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
            _ => new List<int>()
        };
    }
    
    protected virtual List<int> GetSpecificDicesRequired()
    {
        List<int> needDices = new List<int>(needDicesData);
        needDices.RemoveAll(n => diceBox.Contains(n));
        return needDices;
    }
    
    protected virtual List<int> GetSameDicesRequired()
    {
        if (diceBox.Count > 0)
        {
            return new List<int> { diceBox[0] };
        }
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }
    
    protected virtual List<int> GetSumDicesRequired()
    {
        return new List<int> { 1, 2, 3, 4, 5, 6 };
    }
    
    public virtual void Use(bool _isPlayer)
    {
        if (canUseSkill())
        {
            EventCenter.Dispatch(GameEvent.EVENT_SKILL_ATTACK, damage,_isPlayer);
            UnityEngine.Debug.Log($"{skillName} used, dealing {damage} damage!");
        }
        else
        {
            UnityEngine.Debug.Log($"{skillName} cannot be used, insufficient dice!");
        }
        //使用技能後清空骰子
        diceBox.Clear();
    }
}

public class FireBall : BaseSkill
{
    public FireBall()
    {
        skillID = 1;
        skillName = "火球";
        effectText = "造成80點傷害";
        damage = 80f;
        needDicesData = new int[] { 1, 2, 3 };
        requirementType = SkillRequirementType.SpecificDices;  // 設定需求類型
    }
    
    public override bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        return needDicesData.All(n => diceBox.Contains(n));
    }
}

public class Kaminari : BaseSkill
{
    public Kaminari()
    {
        skillID = 2;
        skillName = "雷電";
        conditionText = "相同點數骰子x2";
        effectText = "造成30點傷害";
        damage = 30f;
        requirementType = SkillRequirementType.SameDices;
        requiredSameCount = 2;
    }

    public override bool canUseSkill()
    {
        return diceBox.GroupBy(x => x).Any(g => g.Count() >= requiredSameCount);
    }
}
public class WindBlade : BaseSkill
{
    public WindBlade()
    {
        skillID = 3;
        skillName = "風刃";
        conditionText = "點數總和5以上";
        effectText = "造成10點傷害";
        damage = 10f;
        requirementType = SkillRequirementType.DiceSum;
        requiredSum = 5;
    }

    public override bool canUseSkill()
    {
        return diceBox.Sum() >= requiredSum;
    }
}
public class Punch : BaseSkill
{
    public Punch()
    {
        skillID = 4;
        skillName = "魔力拳";
        conditionText = "任何骰子";
        effectText = "造成骰子點數總和的傷害";
        damage = 0f;
        acceptMoreDice = true;
        requirementType = SkillRequirementType.AnyDice;
    }

    public override bool canUseSkill()
    {
        return diceBox.Count >= 1;
    }
    
    public override void Use(bool _isPlayer)
    {
        damage = diceBox.Sum();
        base.Use(_isPlayer);
    }
}
public class ClawAttack : BaseSkill
{
    public ClawAttack()
    {
        skillID = 5;
        skillName = "爪擊";
        conditionText = "任何骰子";
        effectText = "造成骰子點數總和的兩倍傷害";
        damage = 0f;
        acceptMoreDice = true;
        requirementType = SkillRequirementType.AnyDice;
    }

    public override bool canUseSkill()
    {
        return diceBox.Count >= 1;
    }
    
    public override void Use(bool _isPlayer)
    {
        damage = diceBox.Sum() * 2;
        base.Use(_isPlayer);
    }
}