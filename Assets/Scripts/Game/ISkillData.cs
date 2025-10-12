using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public interface ISkillData
{
    string skillName { get; set; }
    string cardTitle { get; set; }
    float damage { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();
    public void AddDiceData(int _dice);
    public void RemoveDiceData(int _dice);
    public List<int> GetNeedDices();
}

public class FireBall : ISkillData
{
    public string skillName { get; set; } = "火球";
    public string cardTitle { get; set; } = "火球術 1,2,3   造成50點傷害";
    public float damage { get; set; } = 50f;
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDicesData { get; set; } = new int[] { 1, 2, 3 };
    public bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        bool has123 = needDicesData.All(n => diceBox.Contains(n));
        return has123;
    }
    public void AddDiceData(int _dice)
    {
        diceBox.Add(_dice);
    }
    public void RemoveDiceData(int _dice)
    {
        diceBox.Remove(_dice);
    }
    public List<int> GetNeedDices()
    {
        //回傳needDicesData但要移除diceBox已有的
        List<int> needDices = new List<int>(needDicesData);
        needDices.RemoveAll(n => diceBox.Contains(n));
        return needDices;
    }
}

public class Kaminari : ISkillData
{
    public string skillName { get; set; } = "雷電";
    public string cardTitle { get; set; } = "雷電 相同兩個   造成30點傷害";
    public float damage { get; set; } = 30f;
    public List<int> diceBox { get; set; } = new List<int>();
    public bool canUseSkill()
    {
        return diceBox.GroupBy(x => x).Any(g => g.Count() >= 2);
    }
    public void AddDiceData(int _dice)
    {
        diceBox.Add(_dice);
    }
    public void RemoveDiceData(int _dice)
    {
        diceBox.Remove(_dice);
    }
    public List<int> GetNeedDices()
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
public class WindBlade : ISkillData
{
    public string skillName { get; set; } = "風刃";
    public string cardTitle { get; set; } = "風刃 點數>5   造成10點傷害";
    public float damage { get; set; } = 10f;
    public List<int> diceBox { get; set; } = new List<int>();
    public bool canUseSkill()
    {
        return diceBox.Sum() > 5;
    }
    public void AddDiceData(int _dice)
    {
        diceBox.Add(_dice);
    }
    public void RemoveDiceData(int _dice)
    {
        diceBox.Remove(_dice);
    }
    public List<int> GetNeedDices()
    {
        //回傳重複的骰子
        List<int> needDices = new List<int>();
        for (int i = 1; i <= 6; i++)
        {
            needDices.Add(i);
        }
        return needDices;
    }
}