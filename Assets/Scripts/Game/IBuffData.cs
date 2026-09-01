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
    LimitDiceRollResults,
    SpawnBuff,
    EnemyReroll,  // 敵人重新擲骰並再次攻擊
    ClearBuffs, // 清除所有負面 Buff
    Shield
}
public interface IBuffData
{
    int buffID { get; set; }
    string buffName { get; set; }
    string describe { get; set; } // Buff 效果描述
    int usageCount { get; set; } //使用次數
    int duration { get; set; } // 持續時間（回合數）
    List<int> effectValues { get; set; } // Buff 效果數值
    BuffTrigger buffTrigger { get; set; }
    BuffEffectType buffEffectType { get; set; }
    void SetBuffData(int _usageCount, int _duration); // 套用 Buff 效果
    void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character);
    bool CanUseBuff();    //確認使否還能使用
    void DurationDecrease(); //回合結束時減少持續時間
    void RemoveBuffEffect(ICharacterData character); // 移除 Buff 效果
    bool canRemove { get; set; }
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
    public List<int> effectValues { get; set; } = new List<int>();
    private BuffType buffType;
    //紀錄玩家骰子數量
    public int recordedDiceCount { get; set; } = 0;
    public bool canRemove { get; set; } = false;

    // 預設建構子
    public BaseBuff() { }

    // 使用配置數據建構
    public BaseBuff(int buffID, int usageCount, int duration)
    {
        var config = BuffDatabase.GetBuffConfig(buffID);
        ApplyConfig(config);
        SetBuffData(usageCount, duration);
    }

    // 套用配置
    protected void ApplyConfig(BuffConfigData config)
    {
        buffID = config.buffID;
        buffName = config.buffName;
        describe = config.describe;
        buffTrigger = config.buffTrigger;
        buffEffectType = config.buffEffectType;
        effectValues = config.effectValues != null ? new List<int>(config.effectValues) : new List<int>();
    }

    public void SetBuffData(int _usageCount = 0, int _duration = 0)
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
    public void CheckBuffTrigger(BuffTrigger trigger, ICharacterData _character)
    {
        if (trigger == buffTrigger && CanUseBuff())
        {
            UseBuff(_character);
        }
    }
    protected void UseBuff(ICharacterData character)
    {
        EventCenter.Dispatch(GameEvent.EVENT_USE_BUFF, buffName, character.isPlayer);//通知使用buff
        usageCount--;
        switch (buffEffectType)
        {
            case BuffEffectType.HP:
                float _value = -effectValues[0];
                EventCenter.Dispatch(GameEvent.EVENT_BUFF_EFFECT_BLOOD, _value, character.isPlayer);
                Debug.Log($"{buffName} 治療了 {_value} 點生命！");
                break;

            case BuffEffectType.EnemyHP:
                // 對敵人造成傷害（透過事件系統）
                float damageToEnemy = -effectValues[0];
                EventCenter.Dispatch(GameEvent.EVENT_BUFF_EFFECT_BLOOD, damageToEnemy, !character.isPlayer);
                Debug.Log($"{buffName} 對敵人造成了 {damageToEnemy} 點傷害！");
                break;

            case BuffEffectType.BothHP:
                // 同時對自己和對敵人傷害
                float _damage = -effectValues[0];

                EventCenter.Dispatch(GameEvent.EVENT_BUFF_EFFECT_BLOOD, _damage, !character.isPlayer);
                EventCenter.Dispatch(GameEvent.EVENT_BUFF_EFFECT_BLOOD, _damage, character.isPlayer);
                Debug.Log($"對雙方造成 {_damage} 點傷害！");
                break;

            case BuffEffectType.MaxHP:
                // 增加最大生命值
                float maxHpIncrease = effectValues[0];
                character.maxBlood += maxHpIncrease;
                character.currentBlood += maxHpIncrease; // 同時增加當前血量
                EventCenter.Dispatch(GameEvent.EVENT_UPDATE_BLOOD_UI); // 同時增加當前血量
                Debug.Log($"{buffName} 增加了 {maxHpIncrease} 點最大生命！");
                break;

            case BuffEffectType.AttackPower:
                // 增加攻擊力（修改傷害值）
                float attackBonus = effectValues[0];
                character.buffDamage += attackBonus;
                Debug.Log($"{buffName} 增加了 {attackBonus} 點攻擊力！");
                break;

            case BuffEffectType.Defense:
                // 減少受到的傷害
                float damageReduction = effectValues[0];
                character.buffDefense += damageReduction;
                Debug.Log($"{buffName} 減少了 {damageReduction} 點傷害！");
                break;

            case BuffEffectType.Shield:
                Debug.Log($"{buffName} 已準備抵擋下一次攻擊！");
                break;

            case BuffEffectType.BornDice:
                // 額外生成骰子
                int extraDice = (int)effectValues[0];
                character.diceCount += extraDice;
                Debug.Log($"{buffName} 額外獲得 {extraDice} 顆骰子！");
                break;

            case BuffEffectType.LimitBornDice:
                // 限制生成骰子數量
                int diceLimit = (int)effectValues[0];
                character.limitDiceCount = diceLimit;
                Debug.Log($"{buffName} 限制骰子數量為 {diceLimit}！");
                break;

            case BuffEffectType.Stun:
                // 暈眩效果（跳過行動）
                character.state = CharacterState.Stunned;
                Debug.Log($"{buffName} 使目標暈眩，無法行動！");
                break;

            case BuffEffectType.Sleep:
                // 睡眠效果（受到傷害時解除）
                character.state = CharacterState.Sleep;
                Debug.Log($"{buffName} 使目標進入睡眠狀態！");
                break;

            case BuffEffectType.LimitDiceRollResults:
                // 限制骰子結果（例如只能擲出特定點數）
                // effectValues[0] = 最小值, effectValues[1] = 最大值
                character.limitDiceSides = effectValues;

                Debug.Log($"{buffName} 限制骰子點數在 {effectValues} 之間！");
                break;
            case BuffEffectType.SpawnBuff:
                BaseBuff spBuff = new BaseBuff(effectValues[0], effectValues[1], effectValues[2]);
                // 生成新的 Buff 並套用到角色
                EventCenter.Dispatch(GameEvent.EVENT_ADD_BUFF, spBuff, character.isPlayer);
                break;
            case BuffEffectType.EnemyReroll:
                // 敵人重新擲骰並再次攻擊（僅對敵人有效）
                if (!character.isPlayer)
                {
                    EventCenter.Dispatch(GameEvent.EVENT_ENEMY_REROLL);
                    Debug.Log($"{buffName} 觸發敵人重新擲骰！");
                }
                break;
            case BuffEffectType.ClearBuffs:
                EventCenter.Dispatch(GameEvent.EVENT_CLEAR_NEGATIVE_BUFFS, character.isPlayer);
                Debug.Log($"{buffName} 清除了所有負面 Buff！");
                break;
            default:
                Debug.LogWarning($"{buffName} 未處理的 BuffEffectType: {buffEffectType}");
                break;
        }
    }
    public void RemoveBuffEffect(ICharacterData character)
    {
        if (canRemove) return; // 防止重複執行
        canRemove = true;
        if (buffTrigger == BuffTrigger.OnRemove && duration == 0)
        {
            UseBuff(character); // 如果有移除時觸發的效果，先執行一次
        }
        // 根據 buffEffectType 反向移除效果
        switch (buffEffectType)
        {
            case BuffEffectType.MaxHP:
                float maxHpDecrease = effectValues[0];
                character.maxBlood -= maxHpDecrease;
                if (character.currentBlood > character.maxBlood)
                {
                    character.currentBlood = character.maxBlood;
                }
                Debug.Log($"{buffName} 移除了 {maxHpDecrease} 點最大生命！");
                break;
            case BuffEffectType.BornDice:
                int extraDice = (int)effectValues[0];
                character.diceCount -= extraDice;
                if (character.diceCount < 0)
                {
                    character.diceCount = 0;
                }
                Debug.Log($"{buffName} 移除了 {extraDice} 顆骰子！");
                break;
            case BuffEffectType.AttackPower:
                character.buffDamage -= effectValues[0];
                Debug.Log($"{buffName} 移除了攻擊力加成！");
                break;
            case BuffEffectType.Defense:
                character.buffDefense -= effectValues[0];
                Debug.Log($"{buffName} 移除了防禦力加成！");
                break;
            case BuffEffectType.Shield:
                Debug.Log($"{buffName} 抵擋效果已消耗");
                break;
            case BuffEffectType.LimitBornDice:
                character.limitDiceCount = 0;
                Debug.Log($"{buffName} 移除了骰子數量限制！");
                break;
            case BuffEffectType.Stun:
                if (character.state == CharacterState.Stunned)
                {
                    character.state = CharacterState.Idle;
                    Debug.Log($"{buffName} 移除了暈眩效果！");
                }
                break;
            case BuffEffectType.Sleep:
                if (character.state == CharacterState.Sleep)
                {
                    character.state = CharacterState.Idle;
                    Debug.Log($"{buffName} 移除了睡眠效果！");
                }
                break;
            case BuffEffectType.LimitDiceRollResults:
                character.limitDiceSides.Clear();
                Debug.Log($"{buffName} 移除了骰子點數限制！");
                break;
            // 其他效果通常不需要反向移除，因為它們是即時效果
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
