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
                return new GoblinData();
            case 103:
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
    public string spriteLabel = "";
    public int goldReward = 0;
    public virtual void UseSkill()
    {
        //從skillData最後面的開始使用技能 成功使出一個技能就結束
        /*for (int i = skillData.Count - 1; i >= 0; i--)
        {
            skillData[i].diceBox = rollDiceResult;
            skillData[i].Use();
            if (skillData[i].canUseSkill())
            {
                break;
            }
        }*/
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
        spriteLabel = "Slime_G";
        maxBlood = 50f;
        currentBlood = 50f;
        diceSides = new int[] { 1, 2 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new Kaminari() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class GoblinData : EnemyData
{
    public GoblinData()
    {
        enemyId = 2;
        enemyName = "哥布林";
        spriteLabel = "Goblin_G";
        maxBlood = 100f;
        currentBlood = 100f;
        diceSides = new int[] { 1, 2, 3, 4, 5 };
        diceCount = 4;
        skillData = new List<ISkillData>() { new Punch() };
        maxRollCount = 1; //最大擲骰次數
    }
}
public class WolfGirlData : EnemyData
{
    public WolfGirlData()
    {
        enemyId = 103;
        enemyName = "狼女";
        description = "受傷回合，骰子數量變為6，攻擊後回復";
        spriteLabel = "WolfGirl_G";
        maxBlood = 150f;
        currentBlood = 15f;
        diceSides = new int[] { 1, 2, 3, 4 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new ClawAttack() };
        maxRollCount = 1; //最大擲骰次數
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        //一回合只加一次骰子數量
        if (diceCount < 4)
        {
            diceCount += 4;
        }
    }
    public override void UseSkill()
    {
        base.UseSkill();
        if (diceCount >= 4)
        {
            diceCount -= 4;
        }
    }
}
