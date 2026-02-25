using System;
using UnityEngine;
using UnityEngine.UI;

public class HasSkillCard : MonoBehaviour
{
    private bool _isChosen;
    public bool isChosen
    {
        get => _isChosen;
        set
        {
            _isChosen = value;
            img_choose.enabled = value;
        }
    }

    public int SkillID => skillData.skillID;

    [SerializeField] Button btn_card;
    [SerializeField] Image img_choose;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_dice;
    SkillConfigData skillData;

    public void SetData(SkillConfigData _skillData, Action<HasSkillCard> onCardClicked)
    {
        btn_card.onClick.AddListener(() => onCardClicked?.Invoke(this));
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        if (skillData.conditionText == "")
            BurnConditionDices();
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

    void OnEnable()
    {
        EventCenter.AddListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnUnchooseSkill(object[] args)
    {
        int skillID = (int)args[0];
        if (skillID == this.SkillID)
        {
            isChosen = false;
            img_choose.enabled = false;
            Debug.Log("Unchoose skill: " + skillID);
        }
    }
}
