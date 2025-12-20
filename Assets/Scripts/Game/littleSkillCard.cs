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
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_dice;
    [SerializeField] Image img_skillIcon;
    ISkillData skillData;
    public void SetData(ISkillData _skillData)
    {
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        if (skillData.conditionText == "")
            BurnConditionDices();

        img_skillIcon.color = Color.gray; // 預設為灰色，表示未啟用
    }
    //技能開關   
    public void SkillSwitch(bool isOn)
    {
        img_skillIcon.color = isOn ? Color.white : Color.gray;
        //可以加特效
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
}
