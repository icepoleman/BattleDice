using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillCard : MonoBehaviour
{
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_choose;
    [SerializeField] GameObject obj_dice;
    [SerializeField] Button btn_choose;
    [SerializeField] GameObject skillInfoPanel; // 用於顯示技能詳細資訊的面板
    ISkillData skillData;
    public void SetData(ISkillData _skillData)
    {
        skillData = _skillData;
        // skillInfoText.text = skillData.cardTitle;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        if (skillData.conditionText == "")
            BurnConditionDices();
        btn_choose.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, skillData); });
        EventCenter.AddListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDiceEvent);
        EventCenter.AddListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, SkillChoosenEvent);
    }
    void BurnConditionDices()
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
        EventCenter.RemoveListener(GameEvent.EVENT_STOP_USE_DICE, StopUseDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CONFIRM_SELECT_SKILL, SkillChoosenEvent);
    }
    void SkillChoosenEvent(object[] args)
    {
        ISkillData chosenSkill = (ISkillData)args[0];
        obj_choose.SetActive(chosenSkill == skillData);
    }
    void StopUseDiceEvent(object[] args)
    {
        obj_choose.SetActive(false);
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
