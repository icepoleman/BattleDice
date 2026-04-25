using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class ShopSkillCard : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    public int SkillID => skillData.skillID;
    public int Price => skillData.price;
    public bool IsOwned { get; private set; }

    [SerializeField] Button btn_card;
    [SerializeField] TextMeshProUGUI text_skillTitle;
    [SerializeField] TextMeshProUGUI text_skillCondition_title;
    [SerializeField] TextMeshProUGUI text_skillCondition;
    [SerializeField] TextMeshProUGUI text_skillCondition_dice;
    [SerializeField] TextMeshProUGUI text_skillEffect;
    [SerializeField] TextMeshProUGUI text_price;

    [Header("Buff提示")]
    [SerializeField] GameObject obj_buffTip;
    [SerializeField] TextMeshProUGUI text_buffTip;

    SkillConfigData skillData;

    public void SetData(SkillConfigData _skillData, Action<ShopSkillCard> onCardClicked)
    {
        skillData = _skillData;

        btn_card.onClick.AddListener(() => onCardClicked?.Invoke(this));
        text_skillCondition_dice.text = "";
        text_skillCondition_title.text = LanguageManager.GetText("T_Skill_Condition_Title");
        text_skillTitle.text = skillData.skillName;
        text_skillCondition.text = skillData.conditionText;
        text_skillEffect.text = skillData.effectText;

        // 顯示價格
        text_price.text = skillData.price.ToString();

        // 已擁有則按鈕不可點擊
        //btn_card.interactable = !isOwned;

        if (skillData.requirementType == SkillRequirementType.SpecificDices)
            BurnConditionDices();
    }

    void BurnConditionDices()
    {
        text_skillCondition.gameObject.SetActive(false);
        text_skillCondition_dice.text = "";
        for (int i = 0; i < skillData.needDicesData.Length; i++)
        {
            text_skillCondition_dice.text += LanguageManager.GetFormat("Shop_Skill_Condition_Dice", skillData.needDicesData[i]);
        }
    }
    private int currentLinkIndex = -1;
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
