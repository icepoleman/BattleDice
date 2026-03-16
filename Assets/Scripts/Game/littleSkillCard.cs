using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class littleSkillCard : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI text_infoTitle;
    [SerializeField] TextMeshProUGUI text_skillEffect;
    ISkillData skillData;

    [SerializeField] GameObject obj_buffTip;
    [SerializeField] TextMeshProUGUI text_buffTip;
    private int currentLinkIndex = -1;
    [SerializeField] GameObject obj_chooseVfx;
    public void SetData(ISkillData _skillData)
    {
        skillData = _skillData;
        text_infoTitle.text = skillData.skillName;
        text_skillEffect.text += LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect.text += "\n" + skillData.conditionText;
        text_skillEffect.text += "\n" + LanguageManager.GetText("T_skill_effect_title");
        text_skillEffect.text += "\n" + skillData.effectText;
    }
    public int GetSkillID()
    {
        return skillData.skillID;
    }
    //技能開關   
    public void SkillSwitch(bool isOn)
    {
        //img_skillIcon.color = isOn ? Color.white : Color.gray;
        //可以加特效
        obj_chooseVfx.SetActive(isOn);
    }
     #region BUFF說明的Tooltip
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
        //Vector3 tipPosition = _position + new Vector3(1.25F, 0.25f, 0);
        //obj_buffTip.transform.position = tipPosition;

        //Debug.Log($"Show Tooltip: {linkId} at {tipPosition}");
    }

    void HideTooltip()
    {
        Debug.Log("Hide Tooltip");
        obj_buffTip.SetActive(false);
    }
    #endregion
}
