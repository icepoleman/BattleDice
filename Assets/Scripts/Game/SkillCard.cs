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
    [SerializeField] GameObject skillInfoPanel; // 用於顯示技能詳細資訊的面板
    [SerializeField] Toggle tog_choose;
    ISkillData skillData;
    void Awake()
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
        string effectText = skillData.effectText.Replace(",", "\n").Replace("，", "\n");
        text_skillEffect.text = effectText;
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
