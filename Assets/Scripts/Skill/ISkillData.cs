public interface ISkillData
{
    float Damage { get; set; }
    int[] DiceData { get; set; }
    bool CanUseSkill { get; set; }
    public void SetDiceData(int[] newDiceData);
    //發動技能
    public void UseSkill();
    //確認技能條件並發動
    public bool CheckConditionsAndUseSkill();
}
