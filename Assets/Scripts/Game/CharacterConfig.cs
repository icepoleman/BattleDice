using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    
    // 轉換為可存檔的資料
    public virtual CharacterSaveData ToSaveData()
    {
        return new CharacterSaveData
        {
            maxBlood = maxBlood,
            currentBlood = currentBlood,
            diceSides = diceSides,
            diceCount = diceCount,
            keepDiceCount = keepDiceCount,
            maxRollCount = maxRollCount,
            rollDiceResult = new List<int>(rollDiceResult),
            skillIDs = skillData.Select(skill => (int)SkillFactory.GetSkillID(skill)).ToList()
        };
    }
    
    // 從存檔資料載入
    public virtual void LoadFromSaveData(CharacterSaveData saveData)
    {
        maxBlood = saveData.maxBlood;
        currentBlood = saveData.currentBlood;
        diceSides = saveData.diceSides;
        diceCount = saveData.diceCount;
        keepDiceCount = saveData.keepDiceCount;
        maxRollCount = saveData.maxRollCount;
        rollDiceResult = new List<int>(saveData.rollDiceResult);
        
        // 從技能ID重建技能列表
        skillData = saveData.skillIDs
            .Select(id => SkillFactory.CreateSkill((SkillID)id))
            .Where(skill => skill != null)
            .ToList();
    }
}

// 存檔資料類別
[System.Serializable]
public class CharacterSaveData
{
    public float maxBlood;
    public float currentBlood;
    public int[] diceSides;
    public int diceCount;
    public int keepDiceCount;
    public int maxRollCount;
    public List<int> rollDiceResult = new List<int>();
    public List<int> skillIDs = new List<int>(); // 存技能ID
}

public class PlayerData : BaseCharacterData
{
    public PlayerData()
    {
        maxBlood = 100f;
        currentBlood = 100f;
        diceSides = new int[] { 1, 2, 3, 4, 5, 6 };
        diceCount = 3;
        keepDiceCount = 2;
        skillData = new List<ISkillData>() { new FireBall(), new Kaminari(), new Punch() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class SlimeData : BaseCharacterData
{
    public SlimeData()
    {
        maxBlood = 50f;
        currentBlood = 50f;
        diceSides = new int[] { 1, 2 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new Kaminari() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class WolfData : BaseCharacterData
{
    public WolfData()
    {
        maxBlood = 150f;
        currentBlood = 150f;
        diceSides = new int[] { 1, 2, 3, 4 };
        diceCount = 6;
        skillData = new List<ISkillData>() { new Punch() };
        maxRollCount = 1; //最大擲骰次數
    }
}

