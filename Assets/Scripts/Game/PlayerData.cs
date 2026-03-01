using System.Collections.Generic;
using System.Linq;

public class PlayerData : BaseCharacterData
{
    public ISkillData wantUseSkill;
    public int manaRollerMaxDiceCount;
    public PlayerData()
    {
        isPlayer = true;
        diceSides = new int[] { 0, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4, 5, 6 };
        manaRollerMaxDiceCount = 8;

        //test用
        skillIDs = new List<int> { 9,10, 7 };
        diceCount = 8;
        maxRollCount = 3;
        maxBlood = 100f;
        currentBlood = 100f;
        keepDiceCount = 5;

    }
    public override void UseSkill()
    {
        base.UseSkill();
        wantUseSkill.UseSkill(isPlayer);
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