using TMPro;
using UnityEngine;

public class takaraItem : MonoBehaviour
{
    [SerializeField] private GameObject obj_gold;
    [SerializeField] private GameObject obj_gear;
    [SerializeField] private TMP_Text txt_amount;
    [SerializeField] private HasSkillCard skillCard;

    private TreasureBoxReward selectedReward;

    public TreasureBoxReward GetReward()
    {
        return selectedReward;
    }

    public void SetData(TreasureBoxReward reward)
    {
        selectedReward = new TreasureBoxReward
        {
            rewardType = reward.rewardType,
            rewardValue = reward.rewardValue,
            probability = reward.probability
        };

        if (reward.rewardType == TreasureBoxRewardType.Skill)
        {
            SkillConfigData? skillReward = SkillDatabase.GetRandomTreasureSkill("Event");
            if (skillReward == null)
            {
                selectedReward.rewardType = TreasureBoxRewardType.Gear;
                selectedReward.rewardValue = 10;
                obj_gold.SetActive(false);
                obj_gear.SetActive(true);
                skillCard.gameObject.SetActive(false);
                txt_amount.gameObject.SetActive(true);
                txt_amount.text = "10";
                return;
            }

            selectedReward.rewardType = TreasureBoxRewardType.Skill;
            selectedReward.rewardValue = skillReward.Value.skillID;
            obj_gold.SetActive(false);
            obj_gear.SetActive(false);
            skillCard.gameObject.SetActive(true);
            txt_amount.gameObject.SetActive(false);
            skillCard.SetData(skillReward.Value, null);
            return;
        }

        obj_gold.SetActive(reward.rewardType == TreasureBoxRewardType.Gold);
        obj_gear.SetActive(reward.rewardType == TreasureBoxRewardType.Gear);
        skillCard.gameObject.SetActive(false);
        txt_amount.gameObject.SetActive(reward.rewardType != TreasureBoxRewardType.Skill);
        txt_amount.text = "x" + reward.rewardValue.ToString();
    }
}
