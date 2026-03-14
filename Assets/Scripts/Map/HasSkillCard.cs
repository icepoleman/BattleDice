using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class HasSkillCard : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
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
    [SerializeField] TextMeshProUGUI text_skillEffect;
    [SerializeField] Text text_skillEffect_title;
    [SerializeField] Transform trans_skillDiceParent;
    [SerializeField] GameObject obj_dice;
    [SerializeField] GameObject obj_buffTip;
    [SerializeField] TextMeshProUGUI text_buffTip;
    SkillConfigData skillData;
    private int currentLinkIndex = -1;
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

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            text_skillEffect,
            eventData.position,
            eventData.enterEventCamera
        );

        if (linkIndex != -1)
        {
            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;
                TMP_LinkInfo linkInfo = text_skillEffect.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();
                
                // 取得 link 在文字中的位置（使用第一個字符的位置）
                int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
                TMP_CharacterInfo charInfo = text_skillEffect.textInfo.characterInfo[firstCharIndex];
                Vector3 charWorldPos = text_skillEffect.transform.TransformPoint(charInfo.topLeft);
                
                ShowTooltip(linkId, charWorldPos);
            }
        }
        else
        {
            if (currentLinkIndex != -1)
            {
                currentLinkIndex = -1;
                HideTooltip();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentLinkIndex = -1;
        HideTooltip();
    }

    /// <summary>
    /// 顯示彈窗
    /// </summary>
    /// <param name="linkId">link 標籤的 ID，可用於識別要顯示的內容</param>
    /// <param name="position">link 的世界座標位置</param>
    void ShowTooltip(string linkId, Vector3 _position)
    {
        obj_buffTip.SetActive(true);
        BuffConfigData buffData = BuffDatabase.GetBuffConfig(int.Parse(linkId));
        text_buffTip.text = buffData.describe;
        
        // 在 link 上方顯示
        Vector3 tipPosition = _position + new Vector3(1.25F, 0.25f, 0);
        obj_buffTip.transform.position = tipPosition;
        
        Debug.Log($"Show Tooltip: {linkId} at {tipPosition}");
    }

    void HideTooltip()
    {
        Debug.Log("Hide Tooltip");
        obj_buffTip.SetActive(false);
        // TODO: 實作彈窗隱藏
    }
}
