using UnityEngine;

public struct BuffSeed
{
    public int buffID;
    public int usageCount;
    public int duration;

    public BuffSeed(int _buffID, int _usageCount, int _duration)
    {
        buffID = _buffID;
        usageCount = _usageCount;
        duration = _duration;
    }
}
public class BuffFactory
{
    public static IBuffData CreateBuff(int buffID, int usageCount, int duration)
    {
        switch (buffID)
        {
            case 1:  // 魔力護盾
                return new ShieldBuff(usageCount, duration);
            case 2:  // 狂戰士
                return new Berserker(usageCount, duration);
            case 3:  // 狂暴
                return new RageBuff(usageCount, duration);
            case 4:  // 大魔力護盾
                return new ShieldBigBuff(usageCount, duration);
            case 5:  // 魔力重盾
                return new MagicShieldBuff(usageCount, duration);
            case 6:  // 力量增幅
                return new PowerBoost(usageCount, duration);
            case 7:  // 中毒
                return new PoisonBuff(usageCount, duration);
            case 8:  // 流血
                return new BleedBuff(usageCount, duration);
            case 9:  // 暈眩
                return new StunBuff(usageCount, duration);
            case 10: // 睡眠
                return new SleepBuff(usageCount, duration);
            case 11: // 魔力增幅
                return new MagicBoostBuff(usageCount, duration);
            case 12: // 魔力衰弱
                return new MagicWeakenBuff(usageCount, duration);
            case 13: // 肥胖
                return new FatBuff(usageCount, duration);
            case 14: // 惡臭
                return new StinkyBuff(usageCount, duration);
            case 15: // 尖刺護甲
                return new ThornArmorBuff(usageCount, duration);
            default:
                Debug.LogWarning($"未知的 Buff ID: {buffID}");
                return null;
        }
    }
}

public class ShieldBuff : BaseBuff
{
    public ShieldBuff(int _usageCount, int _duration)
    {
        buffID = 1;
        buffName = "魔力護盾";
        describe = "減少5點受到的傷害";
        effectValues.Add(5); // 減少5點傷害
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.Defense;
        SetBuffData(_usageCount, _duration);
    }
}
public class Berserker : BaseBuff
{
    public Berserker(int _usageCount, int _duration)
    {
        buffID = 2;
        buffName = "狂戰士";
        describe = "受傷時，給予狂暴狀態";
        effectValues.Add(3);  // 要生成的 Buff ID（狂暴 = 3）
        effectValues.Add(0);  // 生成 Buff 的使用次數
        effectValues.Add(1);  // 生成 Buff 的持續回合數
        buffTrigger = BuffTrigger.OnDamageTaken;
        buffEffectType = BuffEffectType.SpawnBuff;
        SetBuffData(_usageCount, _duration);
    }
}
public class RageBuff : BaseBuff
{
    public RageBuff(int _usageCount, int _duration)
    {
        buffID = 3;
        buffName = "狂暴";
        describe = "生成骰變為6";
        effectValues.Add(6);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.LimitBornDice;
        SetBuffData(_usageCount, _duration);
    }
}
public class ShieldBigBuff : BaseBuff
{
    public ShieldBigBuff(int _usageCount, int _duration)
    {
        buffID = 4;
        buffName = "大魔力護盾";
        describe = "減少15點受到的傷害";
        effectValues.Add(15);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.Defense;
        SetBuffData(_usageCount, _duration);
    }
}
//魔力重盾
public class MagicShieldBuff : BaseBuff
{
    public MagicShieldBuff(int _usageCount, int _duration)
    {
        buffID = 5;
        buffName = "魔力重盾";
        describe = "減少30點受到的傷害";
        effectValues.Add(30);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.Defense;
        SetBuffData(_usageCount, _duration);
    }
}
//力量增幅
public class PowerBoost : BaseBuff
{
    public PowerBoost(int _usageCount, int _duration)
    {
        buffID = 6;
        buffName = "力量增幅";
        describe = "增加10點攻擊力";
        effectValues.Add(10);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.AttackPower;
        SetBuffData(_usageCount, _duration);
    }
}
//中毒
public class PoisonBuff : BaseBuff
{
    public PoisonBuff(int _usageCount, int _duration)
    {
        buffID = 7;
        buffName = "中毒";
        describe = "每回合結束時受到10點傷害";
        effectValues.Add(-10);
        buffTrigger = BuffTrigger.OnTurnEnd;
        buffEffectType = BuffEffectType.HP;
        SetBuffData(_usageCount, _duration);
    }
}
//Bleed
public class BleedBuff : BaseBuff
{
    public BleedBuff(int _usageCount, int _duration)
    {
        buffID = 8;
        buffName = "流血";
        describe = "使用技能時受到10點傷害";
        effectValues.Add(-10);
        buffTrigger = BuffTrigger.OnSkillUse;
        buffEffectType = BuffEffectType.HP;
        SetBuffData(_usageCount, _duration);
    }
}
//暈眩
public class StunBuff : BaseBuff
{
    public StunBuff(int _usageCount, int _duration)
    {
        buffID = 9;
        buffName = "暈眩";
        describe = "無法行動";
        effectValues.Add(0);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.Stun;
        SetBuffData(_usageCount, _duration);
    }
}
//睡眠
public class SleepBuff : BaseBuff
{
    public SleepBuff(int _usageCount, int _duration)
    {
        buffID = 10;
        buffName = "睡眠";
        describe = "無法行動，直到受到傷害解除";
        effectValues.Add(0);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.Sleep;
        SetBuffData(_usageCount, _duration);
    }
}
//魔力增幅
public class MagicBoostBuff : BaseBuff
{
    public MagicBoostBuff(int _usageCount, int _duration)
    {
        buffID = 11;
        buffName = "魔力增幅";
        describe = "生成魔力骰+1";
        effectValues.Add(1);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.BornDice;
        SetBuffData(_usageCount, _duration);
    }
}
//魔力衰弱
public class MagicWeakenBuff : BaseBuff
{
    public MagicWeakenBuff(int _usageCount, int _duration)
    {
        buffID = 12;
        buffName = "魔力衰弱";
        describe = "生成魔力骰-1";
        effectValues.Add(-1);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.BornDice;
        SetBuffData(_usageCount, _duration);
    }
}
//肥胖
public class FatBuff : BaseBuff
{
    public FatBuff(int _usageCount, int _duration)
    {
        buffID = 13;
        buffName = "肥胖";
        describe = "最大生命+20";
        effectValues.Add(20);
        buffTrigger = BuffTrigger.OnApply;
        buffEffectType = BuffEffectType.MaxHP;
        SetBuffData(_usageCount, _duration);
    }
}
//惡臭
public class StinkyBuff : BaseBuff
{
    public StinkyBuff(int _usageCount, int _duration)
    {
        buffID = 14;
        buffName = "惡臭";
        describe = "回合結束,雙方生命-10";
        effectValues.Add(10);
        buffTrigger = BuffTrigger.OnTurnEnd;
        buffEffectType = BuffEffectType.BothHP;
        SetBuffData(_usageCount, _duration);
    }
}
//尖刺護甲
public class ThornArmorBuff : BaseBuff
{
    public ThornArmorBuff(int _usageCount, int _duration)
    {
        buffID = 15;
        buffName = "尖刺護甲";
        describe = "受到攻擊時，反彈10點傷害給攻擊者";
        effectValues.Add(10);
        buffTrigger = BuffTrigger.OnDamageTaken;
        buffEffectType = BuffEffectType.EnemyHP;
        SetBuffData(_usageCount, _duration);
    }
}