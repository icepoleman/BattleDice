using System;
using UnityEngine;
using UnityEngine.UI;

public class ChooseSkillCard : MonoBehaviour
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
    
    // 點擊回調事件
    public Action<ChooseSkillCard> OnCardClicked;
    
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
    
    void Awake()
    {
        if (btn_card != null)
            btn_card.onClick.AddListener(OnClick);
    }
    
    void OnClick()
    {
        OnCardClicked?.Invoke(this);
    }
    
    public void SetData(SkillConfigData _skillData)
    {
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
}
