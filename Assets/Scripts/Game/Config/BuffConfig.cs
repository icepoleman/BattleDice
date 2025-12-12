using System.Collections.Generic;
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

// Buff 配置結構體
public struct BuffConfigData
{
    // 基本資訊
    public int buffID;                   // Buff 唯一識別碼
    public string buffName;              // Buff 名稱
    public string describe;              // Buff 效果描述

    // 觸發與效果配置
    public BuffTrigger buffTrigger;      // 觸發時機
    public BuffEffectType buffEffectType;// 效果類型
    public int[] effectValues;           // 效果數值列表
}

// Buff 配置資料庫
public static class BuffDatabase
{
    public static readonly Dictionary<int, BuffConfigData> Buffs = new Dictionary<int, BuffConfigData>
    {
        {
            1, new BuffConfigData
            {
                buffID = 1,
                buffName = "魔力護盾",
                describe = "減少5點受到的傷害",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.Defense,
                effectValues = new int[] { 5 }
            }
        },
        {
            2, new BuffConfigData
            {
                buffID = 2,
                buffName = "狂戰士",
                describe = "受傷時，給予狂暴狀態",
                buffTrigger = BuffTrigger.OnDamageTaken,
                buffEffectType = BuffEffectType.SpawnBuff,
                effectValues = new int[] { 3, 0, 1 }  // Buff ID, 使用次數, 持續回合
            }
        },
        {
            3, new BuffConfigData
            {
                buffID = 3,
                buffName = "狂暴",
                describe = "生成骰變為6",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.LimitBornDice,
                effectValues = new int[] { 6 }
            }
        },
        {
            4, new BuffConfigData
            {
                buffID = 4,
                buffName = "大魔力護盾",
                describe = "減少15點受到的傷害",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.Defense,
                effectValues = new int[] { 15 }
            }
        },
        {
            5, new BuffConfigData
            {
                buffID = 5,
                buffName = "魔力重盾",
                describe = "減少30點受到的傷害",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.Defense,
                effectValues = new int[] { 30 }
            }
        },
        {
            6, new BuffConfigData
            {
                buffID = 6,
                buffName = "力量增幅",
                describe = "增加20點攻擊力",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.AttackPower,
                effectValues = new int[] { 20 }
            }
        },
        {
            7, new BuffConfigData
            {
                buffID = 7,
                buffName = "中毒",
                describe = "每回合結束時受到10點傷害",
                buffTrigger = BuffTrigger.OnTurnEnd,
                buffEffectType = BuffEffectType.HP,
                effectValues = new int[] { -10 }
            }
        },
        {
            8, new BuffConfigData
            {
                buffID = 8,
                buffName = "流血",
                describe = "使用技能時受到10點傷害",
                buffTrigger = BuffTrigger.OnSkillUse,
                buffEffectType = BuffEffectType.HP,
                effectValues = new int[] { -10 }
            }
        },
        {
            9, new BuffConfigData
            {
                buffID = 9,
                buffName = "暈眩",
                describe = "無法行動",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.Stun,
                effectValues = new int[] { 0 }
            }
        },
        {
            10, new BuffConfigData
            {
                buffID = 10,
                buffName = "睡眠",
                describe = "無法行動，直到受到傷害解除",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.Sleep,
                effectValues = new int[] { 0 }
            }
        },
        {
            11, new BuffConfigData
            {
                buffID = 11,
                buffName = "魔力增幅",
                describe = "生成魔力骰+1",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.BornDice,
                effectValues = new int[] { 1 }
            }
        },
        {
            12, new BuffConfigData
            {
                buffID = 12,
                buffName = "魔力衰弱",
                describe = "生成魔力骰-1",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.BornDice,
                effectValues = new int[] { -1 }
            }
        },
        {
            13, new BuffConfigData
            {
                buffID = 13,
                buffName = "肥胖",
                describe = "最大生命+20",
                buffTrigger = BuffTrigger.OnApply,
                buffEffectType = BuffEffectType.MaxHP,
                effectValues = new int[] { 20 }
            }
        },
        {
            14, new BuffConfigData
            {
                buffID = 14,
                buffName = "惡臭",
                describe = "回合結束,雙方生命-10",
                buffTrigger = BuffTrigger.OnTurnEnd,
                buffEffectType = BuffEffectType.BothHP,
                effectValues = new int[] { 10 }
            }
        },
        {
            15, new BuffConfigData
            {
                buffID = 15,
                buffName = "尖刺護甲",
                describe = "受到攻擊時，反彈10點傷害給攻擊者",
                buffTrigger = BuffTrigger.OnDamageTaken,
                buffEffectType = BuffEffectType.EnemyHP,
                effectValues = new int[] { 10 }
            }
        },
        {
            16, new BuffConfigData
            {
                buffID = 16,
                buffName = "治癒",
                describe = "回合開始時，生命+20",
                buffTrigger = BuffTrigger.OnTurnStart,
                buffEffectType = BuffEffectType.HP,
                effectValues = new int[] { 20 }
            }
        }
    };

    public static BuffConfigData GetBuffConfig(int buffID)
    {
        if (Buffs.TryGetValue(buffID, out var config))
        {
            return config;
        }
        return default;
    }
}