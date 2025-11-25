using System;
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
public interface IBuffData
{
    int buffID { get; set; }
    string buffName { get; set; }
    string effectText { get; set; } // Buff 效果描述
    int usageCount { get; set; } //使用次數
    int duration { get; set; } // 持續時間（回合數）
    void ApplyBuff(int _usageCount, int _duration); // 套用 Buff 效果
    void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character, ref float value);//value 傳入傷害或治療數值
    bool CanUseBuff();    //確認使否還能使用
    void DurationDecrease(); //回合結束時減少持續時間
}
public class BaseBuff : IBuffData
{
    public int buffID { get; set; } = 0;
    public string buffName { get; set; } = "BaseBuff";
    public string effectText { get; set; } = "";
    public int duration { get; set; } = 1;
    public int usageCount { get; set; } = 1;
    public virtual void ApplyBuff(int _usageCount, int _duration)
    {
        usageCount = _usageCount;
        duration = _duration;
    }
    public virtual void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character, ref float value)
    {

    }
    protected virtual void UseBuff(ref float value)
    {
        usageCount--;
    }
    public virtual bool CanUseBuff()
    {
        return usageCount > 0 && duration > 0;
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
        effectText = "減少受到的傷害";
    }
    public override void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character, ref float value)
    {
        if (trigger == BuffTrigger.OnDamageTaken && CanUseBuff())
        {
            UseBuff(ref value);
            usageCount--;
        }
    }
    protected override void UseBuff(ref float value)
    {
        float damageReduction = 5f; // 減少的傷害值
        value -= damageReduction;
        if (value < 0) value = 0;
        Debug.Log($"{buffName} 減少了 {damageReduction} 點傷害！");
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