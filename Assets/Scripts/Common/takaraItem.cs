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
        var allNotGetSkills = SkillDatabase.GetAllPlayerNotGetSkills();
        selectedReward = new TreasureBoxReward
        {
            rewardType = reward.rewardType,
            rewardValue = reward.rewardValue
        };

        if (reward.rewardType == TreasureBoxRewardType.Skill)
        {
            if (allNotGetSkills == null || allNotGetSkills.Count == 0)
            {
                selectedReward.rewardType = TreasureBoxRewardType.Gold;
                selectedReward.rewardValue = 300;
                obj_gold.SetActive(true);
                obj_gear.SetActive(false);
                skillCard.gameObject.SetActive(false);
                txt_amount.gameObject.SetActive(true);
                txt_amount.text = "x300";
                return;
            }

            var skillReward = allNotGetSkills[Random.Range(0, allNotGetSkills.Count)];
            selectedReward.rewardType = TreasureBoxRewardType.Skill;
            selectedReward.rewardValue = skillReward.skillID;
            obj_gold.SetActive(false);
            obj_gear.SetActive(false);
            skillCard.gameObject.SetActive(true);
            txt_amount.gameObject.SetActive(false);
            skillCard.SetData(skillReward, null);
            return;
        }

        obj_gold.SetActive(reward.rewardType == TreasureBoxRewardType.Gold);
        obj_gear.SetActive(reward.rewardType == TreasureBoxRewardType.Gear);
        skillCard.gameObject.SetActive(false);
        txt_amount.gameObject.SetActive(reward.rewardType != TreasureBoxRewardType.Skill);
        txt_amount.text = "x" + reward.rewardValue.ToString();
    }
}
