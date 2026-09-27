using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class WinLoseView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_title;
    [SerializeField] Button btn_check;
    [SerializeField] Button btn_escape;
    [SerializeField] Button btn_restart;
    [SerializeField] TextMeshProUGUI txt_gold;
    [SerializeField] TextMeshProUGUI txt_gear;
    [SerializeField] HasSkillCard skillCard;

    public void SetData(bool isWin, List<TreasureBoxReward> rewards = null)
    {
        txt_gold.gameObject.SetActive(false);
        txt_gear.gameObject.SetActive(false);
        skillCard.gameObject.SetActive(false);

        if (!isWin)
        {
            txt_title.text = "You Lose!";
            btn_check.gameObject.SetActive(false);
            return;
        }

        txt_title.text = "Win!!!";

        if (rewards == null || rewards.Count == 0)
        {
            btn_check.gameObject.SetActive(true);
            return;
        }

        foreach (var reward in rewards)
        {
            if (reward == null)
                continue;

            switch (reward.rewardType)
            {
                case TreasureBoxRewardType.Gold:
                    txt_gold.text = reward.rewardValue.ToString();
                    txt_gold.gameObject.SetActive(true);
                    break;
                case TreasureBoxRewardType.Gear:
                    txt_gear.text = reward.rewardValue.ToString();
                    txt_gear.gameObject.SetActive(true);
                    break;
                case TreasureBoxRewardType.Skill:
                    var allNotGetSkills = SkillDatabase.GetAllPlayerNotGetSkills();
                    if (allNotGetSkills == null || allNotGetSkills.Count == 0)
                    {
                        skillCard.gameObject.SetActive(false);
                        break;
                    }

                    var skillReward = allNotGetSkills[Random.Range(0, allNotGetSkills.Count)];
                    skillCard.SetData(skillReward, null);
                    skillCard.gameObject.SetActive(true);
                    break;
            }
        }

        btn_check.gameObject.SetActive(true);

        btn_check.onClick.RemoveAllListeners();
        btn_check.onClick.AddListener(() =>
        {
            ClaimRewards(rewards);

            if (!string.IsNullOrEmpty(GameDataManager.TmpCompletedStory))
            {
                string tmpStory = GameDataManager.TmpCompletedStory;
                GameDataManager.TmpCompletedStory = "";
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, tmpStory);
            }
            else
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
        });
        btn_escape.onClick.RemoveAllListeners();
        btn_escape.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(GameEvent.EVENT_ESCAPE_BATTLE);
        });
        btn_restart.onClick.RemoveAllListeners();
        btn_restart.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(GameEvent.EVENT_RESTART_GAME);
        });
    }

    private void ClaimRewards(List<TreasureBoxReward> rewards)
    {
        if (rewards == null || rewards.Count == 0)
            return;

        foreach (var reward in rewards)
        {
            if (reward == null)
                continue;

            switch (reward.rewardType)
            {
                case TreasureBoxRewardType.Gold:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GOLD, reward.rewardValue);
                    break;
                case TreasureBoxRewardType.Gear:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GEAR, reward.rewardValue);
                    break;
                case TreasureBoxRewardType.Skill:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_SKILL, reward.rewardValue);
                    break;
            }
        }
    }
}
