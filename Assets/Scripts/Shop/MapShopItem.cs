using System;
using UnityEngine;
using UnityEngine.UI;

public class MapShopItem : MonoBehaviour
{
    [SerializeField] Text txt_itemName;
    [SerializeField] Text txt_info;
    [SerializeField] Text txt_priceText;
    [SerializeField] Button btn_buy;
    [SerializeField] GameObject obj_soldOut;

    public void SetUp(SkillConfigData skillData)
    {
        string conditionText = string.Format(
            LanguageManager.GetText("T_SkillOnNoraShopCondition"),
            skillData.conditionText,
            skillData.effectText
        );
        txt_itemName.text = skillData.skillName;
        txt_info.text = conditionText;
        txt_priceText.text = skillData.price.ToString();
        btn_buy.onClick.AddListener(() => OnBuySkill(skillData));
    }
    void OnBuySkill(SkillConfigData skillData)
    {
        if (GameDataManager.Gold >= skillData.price)
        {
            EventCenter.Dispatch(MapEvent.EVENT_SPEND_GOLD, skillData.price); // 消耗金幣事件
            EventCenter.Dispatch(MapEvent.EVENT_GET_SKILL, skillData.skillID); //取得技能
            obj_soldOut.SetActive(true);
            btn_buy.interactable = false;
        }
        else
        {
            UIManager.ShowHintBubble(LanguageManager.GetText("T_NotEnoughGold"));
        }
    }
}
