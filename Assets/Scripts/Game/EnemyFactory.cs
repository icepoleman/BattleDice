using System.Collections.Generic;
using UnityEngine;

public static class EnemyFactory
{
    public static EnemyData CreateEnemy(int enemyId)
    {
        switch (enemyId)
        {
            case 1:
                return new SlimeData();
            case 2:
                return new WolfData();
            case 3:
                return new WolfGirlData();
            default:
                Debug.LogError("EnemyFactory: 未知的敵人 ID " + enemyId);
                return null;
        }
    }
}

public class EnemyData : BaseCharacterData
{
    [Header("基本資訊")]
    public int enemyId = 0;
    public string enemyName = "敵人";
    public string description = "";
    public string prefabPath = "";
    public int goldReward = 0;
    public virtual void UseSkill()
    {
        // 預設技能使用邏輯
        skillData[0].diceBox = rollDiceResult;
        skillData[0].Use();
    }
}

public class SlimeData : EnemyData
{
    public SlimeData()
    {
        enemyId = 1;
        enemyName = "史萊姆";
        prefabPath = "character/Slime";
        maxBlood = 50f;
        currentBlood = 50f;
        diceSides = new int[] { 1, 2 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new Kaminari() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class WolfData : EnemyData
{
    public WolfData()
    {
        enemyId = 2;
        enemyName = "哥布林";
        prefabPath = "character/wolf";
        maxBlood = 150f;
        currentBlood = 150f;
        diceSides = new int[] { 1, 2, 3, 4 };
        diceCount = 6;
        skillData = new List<ISkillData>() { new Punch() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class WolfGirlData : EnemyData
{
    public WolfGirlData()
    {
        enemyId = 3;
        enemyName = "狼女";
        description = "受傷會增加骰子數量，使用技能會減少骰子數量。";
        prefabPath = "character/wolfGirl";
        maxBlood = 150f;
        currentBlood = 150f;
        diceSides = new int[] { 1, 2, 3, 4, 5, 6 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new ClawAttack() };
        maxRollCount = 1; //最大擲骰次數
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        diceCount += 6;
    }
    public override void UseSkill()
    {
        base.UseSkill();
        diceCount -= 6;
    }
}
