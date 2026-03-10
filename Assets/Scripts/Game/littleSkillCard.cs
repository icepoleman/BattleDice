using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class littleSkillCard : MonoBehaviour
{
    public int skillID;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Image img_skillIcon;
    [SerializeField] GameObject obj_chooseVfx;
    ISkillData skillData;
    public void SetData(ISkillData _skillData)
    {
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        string effectText = skillData.effectText.Replace(",", "\n").Replace("，", "\n");
        text_skillEffect.text = effectText;
    }
    //技能開關   
    public void SkillSwitch(bool isOn)
    {
        //img_skillIcon.color = isOn ? Color.white : Color.gray;
        //可以加特效
        obj_chooseVfx.SetActive(isOn);
    }
}
