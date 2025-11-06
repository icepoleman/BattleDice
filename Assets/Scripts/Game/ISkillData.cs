using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public enum SkillID
{
    None = 0,
    FireBall = 1,
    Kaminari = 2,
    WindBlade = 3,
    //DeBuffPoison = 4,
    Punch = 4,
    ClawAttack = 5
}

// 技能工廠
public static class SkillFactory
{
    public static ISkillData CreateSkill(SkillID skillId)
    {
        return skillId switch
        {
            SkillID.FireBall => new FireBall(),
            SkillID.Kaminari => new Kaminari(),
            SkillID.WindBlade => new WindBlade(),
            //SkillID.DeBuffPoison => new DeBuffPoison(),
            SkillID.Punch => new Punch(),
            SkillID.ClawAttack => new ClawAttack(),
            _ => null
        };
    }

    public static SkillID GetSkillID(ISkillData skill)
    {
        return skill switch
        {
            FireBall => SkillID.FireBall,
            Kaminari => SkillID.Kaminari,
            WindBlade => SkillID.WindBlade,
            // DeBuffPoison => SkillID.DeBuffPoison,
            Punch => SkillID.Punch,
            ClawAttack => SkillID.ClawAttack,
            _ => SkillID.None
        };
    }
}
public interface ISkillData
{
    SkillID skillID { get; set; }
    bool isDebuff { get; set; }
    bool acceptMoreDice { get; set; } // 新增：是否可以持續放入骰子
    string skillName { get; set; }
    string cardTitle { get; set; }
    float damage { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();
    public void AddDiceData(int _dice);
    public void RemoveDiceData(int _dice);
    public List<int> GetNeedDices();
    public void Use();
}
public class BaseSkill : ISkillData
{
    public SkillID skillID { get; set; } = SkillID.None;
    public bool isDebuff { get; set; } = false;
    public bool acceptMoreDice { get; set; } = false;
    public string skillName { get; set; } = "BaseSkill";
    public string cardTitle { get; set; } = "BaseSkill";
    public float damage { get; set; } = 0f;
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDicesData { get; set; } = new int[] { 1 };
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
        // if (canUseSkill()) return null;
        //回傳needDicesData但要移除diceBox已有的
        List<int> needDices = new List<int>(needDicesData);
        needDices.RemoveAll(n => diceBox.Contains(n));
        return needDices;
    }
    public virtual void Use()
    {
        if (canUseSkill())
        {
            EventCenter.Dispatch(GameEvent.EVENT_SKILL_ATTACK, damage);
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
        skillID = SkillID.FireBall;
        skillName = "火球";
        cardTitle = "火球術 1,2,3   造成80點傷害";
        damage = 80f;
        needDicesData = new int[] { 1, 2, 3 };
    }

    public override bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        return needDicesData.All(n => diceBox.Contains(n));
    }
}
public class DeBuffPoison : BaseSkill
{
    public DeBuffPoison()
    {
        //skillID = SkillID.DeBuffPoison;
        isDebuff = true;
        skillName = "毒素";
        cardTitle = "中毒 1,2,3   回合結束扣除10點生命";
        damage = 10f;
        needDicesData = new int[] { 1, 2, 3 };
    }

    public override bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        return needDicesData.All(n => diceBox.Contains(n));
    }
    public override void Use()
    {
        if (canUseSkill())
            //使用技能後清空骰子 + 移除debuff通知
            base.Use();
        else
            //額外效果: 造成傷害 通知扣血
            UnityEngine.Debug.Log($"{skillName} used, applying poison effect!");
    }
}

public class Kaminari : BaseSkill
{
    public Kaminari()
    {
        skillID = SkillID.Kaminari;
        skillName = "雷電";
        cardTitle = "雷電 相同兩個   造成30點傷害";
        damage = 30f;
    }

    public override bool canUseSkill()
    {
        return diceBox.GroupBy(x => x).Any(g => g.Count() >= 2);
    }

    public override List<int> GetNeedDices()
    {
        //回傳重複的骰子
        List<int> needDices = new List<int>();
        if (diceBox != null && diceBox.Count > 0)
        {
            needDices.Add(diceBox[0]);
        }
        else
        {
            //如果是空就會傳1~6
            for (int i = 1; i <= 6; i++)
            {
                needDices.Add(i);
            }
        }
        if (canUseSkill())
            return new List<int> { 666 };
        return needDices;
    }
}
public class WindBlade : BaseSkill
{
    public WindBlade()
    {
        skillID = SkillID.WindBlade;
        skillName = "風刃";
        cardTitle = "風刃 點數大於6   造成10點傷害";
        damage = 10f;
    }

    public override bool canUseSkill()
    {
        return diceBox.Sum() > 6;
    }

    public override List<int> GetNeedDices()
    {
        //回傳所有骰子
        List<int> needDices = new List<int> { 1, 2, 3, 4, 5, 6 };
        return needDices;
    }
}
public class Punch : BaseSkill
{
    public Punch()
    {
        skillID = SkillID.Punch;
        skillName = "拳頭";
        cardTitle = "拳頭 任何骰子 總和";
        damage = 0f;
        acceptMoreDice = true;
    }

    public override bool canUseSkill()
    {
        return diceBox.Count >= 1;
    }
    public override List<int> GetNeedDices()
    {
        List<int> needDices = new List<int> { 1, 2, 3, 4, 5, 6 };
        return needDices;
    }
    public override void Use()
    {
        damage = diceBox.Sum();
        base.Use();
    }
}
public class ClawAttack : BaseSkill
{
    public ClawAttack()
    {
        skillID = SkillID.ClawAttack;
        skillName = "爪擊";
        cardTitle = "爪擊 任何骰子 總和*2";
        damage = 0f;
        acceptMoreDice = true;
    }

    public override bool canUseSkill()
    {
        return diceBox.Count >= 1;
    }
    public override List<int> GetNeedDices()
    {
        List<int> needDices = new List<int> { 1, 2, 3, 4, 5, 6 };
        return needDices;
    }
    public override void Use()
    {
        damage = diceBox.Sum() * 2;
        base.Use();
    }
}