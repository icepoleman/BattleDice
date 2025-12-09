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
    public int goldReward = 0;
    
    public override void UseSkill()
    {
        base.UseSkill();
        
        // 複製一份骰子結果，用於消耗
        List<int> availableDice = new List<int>(rollDiceResult);
        
        // 從最後一個技能開始檢查（通常是最強的技能）
        for (int i = skillData.Count - 1; i >= 0; i--)
        {
            var skill = skillData[i];
            skill.diceBox.Clear();
            
            // 嘗試用剩餘骰子填充技能
            foreach (int dice in availableDice.ToArray())
            {
                skill.diceBox.Add(dice);
                
                // 如果技能可以使用且不接受更多骰子，立即使用
                if (skill.canUseSkill() && !skill.acceptMoreDice)
                {
                    break;
                }
            }
            
            // 檢查技能是否可以使用
            if (skill.canUseSkill())
            {
                // 使用技能
                skill.Use(isPlayer);
                
                // 從可用骰子中移除已使用的骰子
                foreach (int usedDice in skill.diceBox)
                {
                    availableDice.Remove(usedDice);
                }
                
                // 清空技能的骰子盒
                skill.diceBox.Clear();
                
                Debug.Log($"[Enemy] {enemyName} 使用了 {skill.skillName}，剩餘骰子: {availableDice.Count}");
                
                // 如果沒有剩餘骰子，結束
                if (availableDice.Count == 0)
                {
                    break;
                }
            }
            else
            {
                // 技能無法使用，清空骰子盒繼續檢查下一個
                skill.diceBox.Clear();
            }
        }
    }
}

public class SlimeData : EnemyData
{
    public SlimeData()
    {
        enemyId = 1;
        enemyName = "史萊姆";
        maxBlood = 50f;
        currentBlood = 50f;
        diceSides = new int[] { 1, 2 };
        diceCount = 10;
        skillData = new List<ISkillData>() { SkillFactory.CreateSkill(1), SkillFactory.CreateSkill(2) , SkillFactory.CreateSkill(3) };
       // AddBuff(new Berserker(0, 0));
       // AddBuff(new PowerBoost(0, 3));
        maxRollCount = 1; //最大擲骰次數
    }
}
public class GoblinData : EnemyData
{
    public GoblinData()
    {
        enemyId = 2;
        enemyName = "哥布林";
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
        maxBlood = 150f;
        currentBlood = 15f;
        diceSides = new int[] { 1, 2, 3, 4 };
        diceCount = 2;
        skillData = new List<ISkillData>() { new ClawAttack() };
        buffData = new List<IBuffData>() { };
        maxRollCount = 1; //最大擲骰次數
    }
}
