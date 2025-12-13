using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SkillCard : MonoBehaviour
{
    bool isChosen = false;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_choose;
    [SerializeField] GameObject obj_dice;
    [SerializeField] Button btn_choose;
    [SerializeField] GameObject skillInfoPanel; // 用於顯示技能詳細資訊的面板
    ISkillData skillData;
    public void SetData(ISkillData _skillData, Action onClickCallback = null)
    {
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        if (skillData.conditionText == "")
            BurnConditionDices();
        btn_choose.onClick.AddListener(() => { 
           // isChosen = true;
           // skillInfoPanel.SetActive(true);
            onClickCallback?.Invoke();
        });
        EventCenter.AddListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, StopUseSkill);
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
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, StopUseSkill);
    }
    public void SkillChoosenEvent()
    {
        obj_choose.SetActive(true);
    }
    void StopUseSkill(object[] args)
    {
        obj_choose.SetActive(false);
    }

    // 滑鼠進入按鈕時觸發
    public void OnMouseEnter()
    {
       // skillInfoPanel.SetActive(true);
    }

    // 滑鼠離開按鈕時觸發
    public void OnMouseExit()
    {
       // if (!isChosen)
       // skillInfoPanel.SetActive(false);
    }
}
