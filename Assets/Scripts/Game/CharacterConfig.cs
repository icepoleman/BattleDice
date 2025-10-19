using UnityEngine;
using System.Collections.Generic;

public interface ICharacterData
{
    float maxBlood { get; set; }
    float currentBlood { get; set; }
    int[] diceSides { get; set; }
    int diceCount { get; set; }
    int keepDiceCount { get; set; }
    List<ISkillData> skillData { get; set; }
    int maxRollCount { get; set; } //最大擲骰次數
    List<int> rollDiceResult { get; set; }
    void TakeDamage(float damage);
    void Heal(float heal);
    List<int> RollDice();
    bool IsDead();
}

// 基礎角色類別，實作共同邏輯
public abstract class BaseCharacterData : ICharacterData
{
    public float maxBlood { get; set; }
    public float currentBlood { get; set; }
    public int[] diceSides { get; set; }
    public int diceCount { get; set; }
    public int keepDiceCount { get; set; }
    public List<ISkillData> skillData { get; set; }
    public int maxRollCount { get; set; }
    public List<int> rollDiceResult { get; set; } = new List<int>();
    
    public virtual void TakeDamage(float damage)
    {
        currentBlood -= damage;
        if (currentBlood < 0) currentBlood = 0;
    }
    
    public virtual void Heal(float heal)
    {
        currentBlood += heal;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    
    public virtual List<int> RollDice()
    {
        rollDiceResult.Clear(); // 清空之前的結果
        for (int i = 0; i < diceCount; i++)
        {
            int side = diceSides[Random.Range(0, diceSides.Length)];
            rollDiceResult.Add(side);
        }
        return rollDiceResult;
    }
    
    public virtual bool IsDead()
    {
        return currentBlood <= 0;
    }
}
public class PlayerData : BaseCharacterData
{
    public PlayerData()
    {
        maxBlood = 100f;
        currentBlood = 100f;
        diceSides = new int[] { 1, 2, 3, 4, 5, 6 };
        diceCount = 5;
        keepDiceCount = 2;
        skillData = new List<ISkillData>() { new FireBall(), new Kaminari() };
        maxRollCount = 3; //最大擲骰次數
    }
}
public class SlimeData : BaseCharacterData
{
    public SlimeData()
    {
        maxBlood = 50f;
        currentBlood = 50f;
        diceSides = new int[] { 1, 2, 3 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new Kaminari() };
        maxRollCount = 1; //最大擲骰次數
    }
}


