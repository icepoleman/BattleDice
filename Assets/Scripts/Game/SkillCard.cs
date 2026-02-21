using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SkillCard : MonoBehaviour
{
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_dice;
    [SerializeField] GameObject skillInfoPanel; // 用於顯示技能詳細資訊的面板
    [SerializeField] Toggle tog_choose;
    ISkillData skillData;
    void Start()
    {
        tog_choose.gameObject.SetActive(false);
    }
    public void SetData(ISkillData _skillData, ToggleGroup _toggleGroup = null)
    {
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        if (skillData.conditionText == "")
            BurnConditionDices();
        tog_choose.group = _toggleGroup;
        tog_choose.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
            {
                skillInfoPanel.SetActive(false);
                return;
            }
            skillInfoPanel.SetActive(true);
            EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, _skillData);
        });
        tog_choose.gameObject.SetActive(true);
    }
    void BurnConditionDices()//如果沒有條件骰子就不顯示
    {
        text_skillCondition.gameObject.SetActive(false);
        for (int i = 0; i < skillData.needDicesData.Length; i++)
        {
            int sideNum = skillData.needDicesData[i];
            GameObject diceObj = Instantiate(obj_dice, trans_skillDiceParent);
            diceObj.SetActive(true);
            Image img = diceObj.GetComponent<Image>();
            img.sprite = ResourcesLoader.GetDiceSprite(sideNum);
        }
    }
    //開關卡片使用interactable
    public void SetInteractable(bool _isInteractable)
    {
        tog_choose.interactable = _isInteractable;
    }
    // 滑鼠進入按鈕時觸發
    public void OnMouseEnter()
    {
        skillInfoPanel.SetActive(true);
    }

    // 滑鼠離開按鈕時觸發
    public void OnMouseExit()
    {
        skillInfoPanel.SetActive(false);
    }
}
