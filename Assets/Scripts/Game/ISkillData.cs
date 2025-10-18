using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public interface ISkillData
{
    bool isDebuff { get; set; }
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
    public bool isDebuff { get; set; } = false;
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
        //回傳needDicesData但要移除diceBox已有的
        List<int> needDices = new List<int>(needDicesData);
        needDices.RemoveAll(n => diceBox.Contains(n));
        return needDices;
    }
    public virtual void Use()
    {
        //使用技能後清空骰子
        diceBox.Clear();
    }
}

public class FireBall : BaseSkill
{
    public FireBall()
    {
        skillName = "火球";
        cardTitle = "火球術 1,2,3   造成50點傷害";
        damage = 50f;
        needDicesData = new int[] { 1, 2, 3 };
    }

    public override bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        return needDicesData.All(n => diceBox.Contains(n));
    }
    public override void Use()
    {
        //使用技能後清空骰子
        base.Use();
        //額外效果: 造成傷害
        UnityEngine.Debug.Log($"{skillName} used, dealing {damage} damage!");
    }
}
public class DeBuffPoison : BaseSkill
{
    public DeBuffPoison()
    {
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
        return needDices;
    }
}
public class WindBlade : BaseSkill
{
    public WindBlade()
    {
        skillName = "風刃";
        cardTitle = "風刃 點數>3   造成10點傷害";
        damage = 10f;
    }
    
    public override bool canUseSkill()
    {
        return diceBox.Sum() > 3;
    }
    
    public override List<int> GetNeedDices()
    {
        //回傳所有骰子
        List<int> needDices = new List<int>();
        for (int i = 1; i <= 6; i++)
        {
            needDices.Add(i);
        }
        return needDices;
    }
}