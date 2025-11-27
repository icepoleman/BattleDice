using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public interface ICharacterData
{
    bool isPlayer { get; set; }
    float maxBlood { get; set; }
    float currentBlood { get; set; }
    int[] diceSides { get; set; }
    int diceCount { get; set; }
    int keepDiceCount { get; set; }
    List<ISkillData> skillData { get; set; }
    List<IBuffData> buffData { get; set; }
    int maxRollCount { get; set; } //最大擲骰次數
    List<int> rollDiceResult { get; set; }
    void TakeDamage(float damage);
    void Heal(float heal);
    void Attack(float damage);
    void UseSkill();//TODO:新增使用技能介面
    void ApplyBuff(IBuffData buff);//新增套用Buff介面
    void RemoveBuff(IBuffData buff);//新增移除Buff介面
    List<int> RollDice();
    bool IsDead();
    void TurnEndBuffDecrease(); //回合結束時Buff持續時間減少
}

// 基礎角色類別，實作共同邏輯
public abstract class BaseCharacterData : ICharacterData
{
    public bool isPlayer { get; set; } = false;
    public float maxBlood { get; set; }
    public float currentBlood { get; set; }
    public int[] diceSides { get; set; }
    public int diceCount { get; set; }
    public int keepDiceCount { get; set; }
    public List<ISkillData> skillData { get; set; } = new List<ISkillData>();
    public List<IBuffData> buffData { get; set; } = new List<IBuffData>();
    public int maxRollCount { get; set; }
    public List<int> rollDiceResult { get; set; } = new List<int>();
    public virtual List<int> RollDice()
    {
        rollDiceResult.Clear(); // 清空之前的結果
        for (int i = 0; i < diceCount; i++)
        {
            int side = diceSides[UnityEngine.Random.Range(0, diceSides.Length)];
            rollDiceResult.Add(side);
        }
        return rollDiceResult;
    }
    public virtual void UseSkill()
    {
        float tmp = 0f;
        foreach (var buff in buffData)
        {
            buff.CheckBuffTrigger(BuffTrigger.OnSkillUse, this, ref tmp);
        }
    }
    public virtual void Attack(float damage)
    {
        float tmp = damage;
        foreach (var buff in buffData)
        {
            buff.CheckBuffTrigger(BuffTrigger.OnAttactk, this, ref tmp);
        }
        EventCenter.Dispatch(GameEvent.EVENT_ATTACK_CHARACTER, tmp, !isPlayer);
    }
    public virtual void TakeDamage(float damage)
    {
        foreach (var buff in buffData)
        {
            buff.CheckBuffTrigger(BuffTrigger.OnDamageTaken, this, ref damage);
        }
        currentBlood -= damage;
        if (currentBlood < 0) currentBlood = 0;
    }

    public virtual void Heal(float heal)
    {
        foreach (var buff in buffData)
        {
            buff.CheckBuffTrigger(BuffTrigger.OnHealReceived, this, ref heal);
        }
        currentBlood += heal;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    public virtual void ApplyBuff(IBuffData buff)
    {
        buffData.Add(buff);
        float dummyValue = 0f;
        buff.CheckBuffTrigger(BuffTrigger.OnApply, this, ref dummyValue);
    }
    public virtual void RemoveBuff(IBuffData buff)
    {
        buffData.Remove(buff);
        float dummyValue = 0f;
        buff.CheckBuffTrigger(BuffTrigger.OnRemove, this, ref dummyValue);
    }
    public virtual bool IsDead()
    {
        return currentBlood <= 0;
    }
    public virtual void TurnEndBuffDecrease()
    {
        foreach (var buff in buffData)
        {
            buff.DurationDecrease();
        }
        buffData.RemoveAll(buff => !buff.CanUseBuff());
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
    public List<int> skillIDs = new List<int>(); // 存技能ID
}

public class PlayerData : BaseCharacterData
{
    public ISkillData wantUseSkill;
    public PlayerData()
    {
        isPlayer = true;
        maxBlood = 100f;
        currentBlood = 100f;
        diceSides = new int[] { 1, 2, 3, 4, 5, 6 };
        diceCount = 6;
        keepDiceCount = 2;
        skillData = new List<ISkillData>() { new FireBall(), new Kaminari(), new WindBlade() };
        buffData = new List<IBuffData>() { new ShieldBuff() };
        maxRollCount = 1; //最大擲骰次數
    }
    public void AddPowerDice(int dice)
    {
        wantUseSkill.diceBox.Add(dice);
        //達成條件直接使用技能
        if (wantUseSkill.canUseSkill() && wantUseSkill.acceptMoreDice == false)
        {
            EventCenter.Dispatch(GameEvent.EVENT_PLAYER_USE_SKILL);
        }
    }
    public override void UseSkill()
    {
        base.UseSkill();
        wantUseSkill.Use(isPlayer);
    }
    // 轉換為可存檔的資料
    public CharacterSaveData ToSaveData()
    {
        return new CharacterSaveData
        {
            maxBlood = maxBlood,
            currentBlood = currentBlood,
            diceSides = diceSides,
            diceCount = diceCount,
            keepDiceCount = keepDiceCount,
            maxRollCount = maxRollCount,
            skillIDs = skillData.Select(skill => skill.skillID).ToList()
        };
    }

    // 從存檔資料載入
    public void LoadFromSaveData(CharacterSaveData saveData)
    {
        maxBlood = saveData.maxBlood;
        currentBlood = saveData.currentBlood;
        diceSides = saveData.diceSides;
        diceCount = saveData.diceCount;
        keepDiceCount = saveData.keepDiceCount;
        maxRollCount = saveData.maxRollCount;

        // 從技能ID重建技能列表
        skillData = saveData.skillIDs
            .Select(id => SkillFactory.CreateSkill(id))
            .Where(skill => skill != null)
            .ToList();
    }
}

