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
                ShowTooltip(linkId, eventData.position);
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
    /// <param name="position">滑鼠位置</param>
    void ShowTooltip(string linkId, Vector2 position)
    {
        Debug.Log($"Show Tooltip: {linkId} at {position}");
        // TODO: 實作彈窗顯示
    }

    void HideTooltip()
    {
        Debug.Log("Hide Tooltip");
        // TODO: 實作彈窗隱藏
    }
}
