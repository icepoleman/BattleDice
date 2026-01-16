using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopSkillCard : MonoBehaviour
{
    public int SkillID => skillData.skillID;
    public int Price => skillData.price;
    public bool IsOwned { get; private set; }
    
    [SerializeField] Button btn_card;
    [SerializeField] Text text_skillTitle;
    [SerializeField] Text text_skillCondition_title;
    [SerializeField] Text text_skillCondition;
    [SerializeField] Text text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Text text_price;
    [SerializeField] GameObject obj_owned;  // 已擁有標記
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_dice;
    
    SkillConfigData skillData;
    
    public void SetData(SkillConfigData _skillData, bool isOwned, Action<ShopSkillCard> onCardClicked)
    {
        skillData = _skillData;
        IsOwned = isOwned;
        
        btn_card.onClick.AddListener(() => onCardClicked?.Invoke(this));
        
        text_skillCondition_title.text = LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect_title.text = LanguageManager.GetText("T_skill_effect_title");
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;
        
        // 顯示價格
        text_price.text = skillData.price.ToString();
        
        // 已擁有狀態
        if (obj_owned != null)
        {
            obj_owned.SetActive(isOwned);
        }
        
        // 已擁有則按鈕不可點擊
        btn_card.interactable = !isOwned;
        
        if (skillData.conditionText == "")
            BurnConditionDices();
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
}
