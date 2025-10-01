using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public interface ISkillData
{
    string skillName { get; set; }
    float damage { get; set; }
    List<int> diceBox { get; set; }
    public bool canUseSkill();

    public void AddDiceData(int _dice);
}

public class FireBall : ISkillData
{
    public string skillName { get; set; } = "火球";
    public float damage { get; set; } = 50f;
    public List<int> diceBox { get; set; } = new List<int>();
    public int[] needDiceData1 { get; set; } = new int[] { 1, 2, 3 };
    public int[] needDiceData2 { get; set; } = new int[] { 4, 5, 6 };
    public bool canUseSkill()
    {
        // 檢查是否同時有 1,2,3
        bool has123 = needDiceData1.All(n => diceBox.Contains(n));
        // 檢查是否同時有 4,5,6
        bool has456 = needDiceData2.All(n => diceBox.Contains(n));
        return has123 || has456;
    }
    public void AddDiceData(int _dice)
    {
        diceBox.Add(_dice);
    }
}

public class Kaminari : ISkillData
{
    public string skillName { get; set; } = "雷電";
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
}
public class WindBlade : ISkillData
{
    public string skillName { get; set; } = "風刃";
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
}