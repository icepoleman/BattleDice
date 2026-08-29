using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
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
        Debug.Log($"限制骰子數量: {limitDiceCount}");
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
        rollDiceResult.Sort();
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
    public virtual async void TakeDamage(float damage)
    {
        // 受到傷害時解除睡眠狀態
        if (state == CharacterState.Sleep)
        {
            RemoveSleepBuff();
        }
        if (damage > 0)
            TriggerBuffs(BuffTrigger.OnDamageTaken);
        await Task.Yield(); // 確保Buff觸發效果先執行
        float takeDmg = damage - buffDefense;
        if (takeDmg < 0) takeDmg = 0;
        if (damage < 0) takeDmg = damage;//如果是治療則不受防禦力影響
        Debug.Log($"{(isPlayer ? "玩家" : "敵人")} 受到 {takeDmg} 點傷害，防禦力 {buffDefense}");
        currentBlood -= takeDmg;
        if (currentBlood < 0) currentBlood = 0;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    public void RemoveSleepBuff()
    {
        var sleepBuff = buffData.FirstOrDefault(b => b.buffEffectType == BuffEffectType.Sleep);
        if (sleepBuff != null)
        {
            RemoveBuff(sleepBuff);
        }
    }
    public virtual void Heal(float heal)
    {
        TriggerBuffs(BuffTrigger.OnHealReceived);
        currentBlood += heal;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    public virtual void AddBuff(IBuffData buff)
    {
        // 同一個 Buff 類型可重複套用，且每個實例都是獨立存在，
        // 不再把不同的同類 Buff 合併成一個 duration 累加的單一物件。
        // 這樣重複中毒 3 / 3 會變成兩個獨立的中毒狀態，都會在各自回合觸發。
        buff.CheckBuffTrigger(BuffTrigger.OnApply, this);
        buffData.Add(buff);
    }
    public virtual void RemoveBuff(IBuffData buff)//只被動作內部呼叫
    {
        Debug.Log($"移除Buff: {buff.buffID}");
       // buff.CheckBuffTrigger(BuffTrigger.OnRemove, this);
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



