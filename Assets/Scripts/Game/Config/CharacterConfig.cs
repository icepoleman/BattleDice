using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
public enum CharacterState
{
    Idle,
    Stunned,
    Sleep
}
public interface ICharacterData
{
    CharacterState state { get; set; }
    bool isPlayer { get; set; }
    float maxBlood { get; set; }
    float currentBlood { get; set; }
    int[] diceSides { get; set; }
    List<int> limitDiceSides { get; set; }
    int diceCount { get; set; }
    int limitDiceCount { get; set; }//限制生成骰子數量
    int keepDiceCount { get; set; }
    List<int> skillIDs { get; set; } 
    List<ISkillData> skillData { get; set; }
    List<IBuffData> buffData { get; set; }
    int maxRollCount { get; set; } //最大擲骰次數
    List<int> rollDiceResult { get; set; }
    float buffDamage { get; set; }
    float buffDefense { get; set; }
    void TakeDamage(float damage);
    void Heal(float heal);
    void Attack(float damage);
    void UseSkill();//TODO:新增使用技能介面
    void AddBuff(IBuffData buff);//新增套用Buff介面
    void RemoveBuff(IBuffData buff);//新增移除Buff介面
    List<int> RollDice();
    bool IsDead();
    void TurnEndBuffDecrease(); //回合結束時Buff持續時間減少
    void TurnStartBuffEffect(); //回合開始時Buff效果觸發
}

// 基礎角色類別，實作共同邏輯
public abstract class BaseCharacterData : ICharacterData
{
    public CharacterState state { get; set; } = CharacterState.Idle;
    public bool isPlayer { get; set; } = false;
    public float maxBlood { get; set; }
    public float currentBlood { get; set; }
    public int[] diceSides { get; set; }
    public List<int> limitDiceSides { get; set; } = new List<int>();
    public int diceCount { get; set; }
    public int limitDiceCount { get; set; }//限制生成骰子數量
    public int keepDiceCount { get; set; }
    private List<int> _skillIDs = new List<int>();
    public List<int> skillIDs
    {
        get => _skillIDs;
        set
        {
            _skillIDs = value;
            // 依照 skillIDs 初始化 skillData
            skillData.Clear();
            foreach (var id in _skillIDs)
            {
                skillData.Add(new BaseSkill(id));
            }
        }
    }
    public List<ISkillData> skillData { get; set; } = new List<ISkillData>();
    public List<IBuffData> buffData { get; set; } = new List<IBuffData>();
    public int maxRollCount { get; set; }
    public List<int> rollDiceResult { get; set; } = new List<int>();
    public float buffDamage { get; set; } = 0f;
    public float buffDefense { get; set; } = 0f;
    public virtual List<int> RollDice()
    {
        int useDice = diceCount;
        if (limitDiceCount > 0)//有設定限制數量時使用
        {
            useDice = limitDiceCount;
        }
        rollDiceResult.Clear(); // 清空之前的結果
        for (int i = 0; i < useDice; i++)
        {
            int side = diceSides[UnityEngine.Random.Range(0, diceSides.Length)];
            if (limitDiceSides.Count > 0)//有設定限制點數時使用
            {
                side = limitDiceSides[UnityEngine.Random.Range(0, limitDiceSides.Count)];
            }
            rollDiceResult.Add(side);
        }
        return rollDiceResult;
    }

    /// <summary>
    /// 觸發所有 Buff 的指定事件
    /// </summary>
    protected void TriggerBuffs(BuffTrigger trigger)
    {
        foreach (var buff in buffData)
        {
            buff.CheckBuffTrigger(trigger, this);
        }
        RemoveInvalidBuffs();
    }

    public virtual void UseSkill()
    {
        TriggerBuffs(BuffTrigger.OnSkillUse);
    }
    public virtual void Attack(float damage)
    {
        TriggerBuffs(BuffTrigger.OnAttactk);
        EventCenter.Dispatch(GameEvent.EVENT_ATTACK_CHARACTER, damage + buffDamage, !isPlayer);
    }
    public virtual void TakeDamage(float damage)
    {
        // 受到傷害時解除睡眠狀態
        if (state == CharacterState.Sleep)
        {
            var sleepBuff = buffData.FirstOrDefault(b => b.buffEffectType == BuffEffectType.Sleep);
            if (sleepBuff != null)
            {
                RemoveBuff(sleepBuff);
            }
        }
        TriggerBuffs(BuffTrigger.OnDamageTaken);
        float takeDmg = damage - buffDefense;
        if (takeDmg < 0) takeDmg = 0;
        Debug.Log($"{(isPlayer ? "玩家" : "敵人")} 受到 {takeDmg} 點傷害，防禦力 {buffDefense}");
        currentBlood -= takeDmg;
        if (currentBlood < 0) currentBlood = 0;
    }

    public virtual void Heal(float heal)
    {
        TriggerBuffs(BuffTrigger.OnHealReceived);
        currentBlood += heal;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    public virtual void AddBuff(IBuffData buff)
    {
        //如果已經有相同的 Buff ID，則刷新效果
        if (buffData.Any(b => b.buffID == buff.buffID))
        {
            buffData.Remove(buffData.First(b => b.buffID == buff.buffID));
        }
        buff.CheckBuffTrigger(BuffTrigger.OnApply, this);
        buffData.Add(buff);
    }
    public virtual void RemoveBuff(IBuffData buff)//只被動作內部呼叫
    {
        buff.CheckBuffTrigger(BuffTrigger.OnRemove, this);
        buff.RemoveBuffEffect(this);
    }
    public virtual bool IsDead()
    {
        return currentBlood <= 0;
    }
    public virtual void TurnStartBuffEffect()
    {
        TriggerBuffs(BuffTrigger.OnTurnStart);
    }
    public virtual void TurnEndBuffDecrease()
    {
        TriggerBuffs(BuffTrigger.OnTurnEnd);
        foreach (var buff in buffData)
        {
            buff.DurationDecrease();
        }
        RemoveInvalidBuffs();
    }
    // 移除無效 Buff
    public void RemoveInvalidBuffs()
    {
        foreach (var buff in buffData)
        {
            if (!buff.CanUseBuff())
            {
                RemoveBuff(buff);//先執行效果
            }
        }
        UpdateBuffUI();
    }
    //移除所有buff
    public void RemoveAllBuff() 
    {
        foreach (var buff in buffData.ToList()) // 使用 ToList() 以避免修改集合時出錯
        {
            RemoveBuff(buff);
        }
        buffData.Clear();
        UpdateBuffUI();
    }
    public void UpdateBuffUI()
    {
        // 移除 canRemove == true 的 Buff，保留 canRemove == false 的
        buffData = buffData.Where(b => !b.canRemove).ToList();

        EventCenter.Dispatch(GameEvent.EVENT_UPDATE_BUFF);
    }
}

// 存檔資料類別
[Serializable]
public class CharacterSaveData
{
    public float maxBlood;
    public float currentBlood;
    public int[] diceSides;
    public int diceCount;
    public int keepDiceCount;
    public int maxRollCount;
    public int manaRollerMaxDiceCount;
    public List<int> skillIDs = new List<int>(); // 存技能ID
}

public class PlayerData : BaseCharacterData
{
    public ISkillData wantUseSkill;
    public int manaRollerMaxDiceCount;
    public PlayerData()
    {
        isPlayer = true;
        maxBlood = 200f;
        currentBlood = 200f;
        diceSides = new int[] { 1, 2, 3, 4, 5, 6 };
        diceCount = 8;
        keepDiceCount = 2;
        manaRollerMaxDiceCount = 8;
        skillIDs = new List<int>() { 6, 12, 13, 11 };
        maxRollCount = 10; //最大擲骰次數
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
            manaRollerMaxDiceCount = manaRollerMaxDiceCount,
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
        manaRollerMaxDiceCount = saveData.manaRollerMaxDiceCount;
        // 從技能ID重建技能列表（透過 setter 自動初始化）
        skillIDs = saveData.skillIDs;
    }
}

