using System;
using System.Collections.Generic;
using UnityEngine;
public enum BuffTrigger
{
    OnApply,
    OnRemove,
    OnTurnStart,
    OnTurnEnd,
    OnDamageTaken,
    OnHealReceived,
    OnSkillUse,
    OnAttactk
}
public enum BuffType
{
    Timed,                // 時間倒數完消失
    ChargeBased,          // 次數用完消失
    Permanent,            // 永久存在
    TimedAndChargeBased   // 其中一項歸零就消失
}
public enum BuffEffectType
{
    HP,
    EnemyHP,
    BothHP,
    MaxHP,
    AttackPower,
    Defense,
    BornDice,
    LimitBornDice,
    Stun,
    Sleep,
    LimitDiceRollResults
}
public interface IBuffData
{
    int buffID { get; set; }
    string buffName { get; set; }
    string describe { get; set; } // Buff 效果描述
    int usageCount { get; set; } //使用次數
    int duration { get; set; } // 持續時間（回合數）
    List<float> effectValues { get; set; } // Buff 效果數值
    BuffTrigger buffTrigger { get; set; }
    BuffEffectType buffEffectType { get; set; }
    void ApplyBuff(int _usageCount, int _duration); // 套用 Buff 效果
    void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character, ref float value);//value 傳入傷害或治療數值
    bool CanUseBuff();    //確認使否還能使用
    void DurationDecrease(); //回合結束時減少持續時間
}
public class BaseBuff : IBuffData
{
    public int buffID { get; set; } = 0;
    public string buffName { get; set; } = "BaseBuff";
    public string describe { get; set; } = "";
    public int duration { get; set; } = 1;
    public int usageCount { get; set; } = 1;
    public BuffTrigger buffTrigger { get; set; }
    public BuffEffectType buffEffectType { get; set; }
    public List<float> effectValues { get; set; } = new List<float>();
    private BuffType buffType;
    public void ApplyBuff(int _usageCount = 0, int _duration = 0)
    {
        usageCount = _usageCount;
        duration = _duration;
        if (_usageCount > 0 && _duration > 0)
        {
            buffType = BuffType.TimedAndChargeBased;
        }
        else if (_usageCount > 0)
        {
            buffType = BuffType.ChargeBased;
        }
        else if (_duration > 0)
        {
            buffType = BuffType.Timed;
        }
        else
        {
            buffType = BuffType.Permanent;
        }
        Debug.Log($"{buffName} 已套用，類型: {buffType}, 使用次數: {usageCount}, 持續時間: {duration} 回合");
    }
    public void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character, ref float value)
    {
        if (trigger == buffTrigger && CanUseBuff())
        {
            UseBuff(ref value);
        }
    }
    protected void UseBuff(ref float value)
    {
        usageCount--;
        switch (buffEffectType)
        {
            case BuffEffectType.Defense:
                value -= effectValues[0];
                if (value < 0) value = 0;
                Debug.Log($"{buffName} 減少了 {effectValues[0]} 點傷害！");
                break;
            // 可以擴展其他 BuffType 的效果
            default:
                break;
        }
    }
    public bool CanUseBuff()
    {
        switch (buffType)
        {
            case BuffType.Permanent:
                return true;
            case BuffType.ChargeBased:
                return usageCount > 0;
            case BuffType.Timed:
                return duration > 0;
            case BuffType.TimedAndChargeBased:
                return usageCount > 0 && duration > 0;
            default:
                return false;
        }
    }
    public void DurationDecrease()
    {
        duration--;
    }
}
public class ShieldBuff : BaseBuff
{
    public ShieldBuff()
    {
        buffID = 1;
        buffName = "護盾";
        describe = "減少受到的傷害";
        buffTrigger = BuffTrigger.OnDamageTaken;
        buffEffectType = BuffEffectType.Defense;
    }
}
public class BuffFactory
{
    public static IBuffData CreateBuff(int buffId)
    {
        switch (buffId)
        {
            case 1:
                return new ShieldBuff();
            case 2:
            // return new RegenerationBuff();
            default:
                Debug.LogError("BuffFactory: 未知的 Buff ID " + buffId);
                return null;
        }
    }
}