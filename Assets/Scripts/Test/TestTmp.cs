using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// TMP sprite 懸停彈窗測試
/// 使用方式：在 TMP 文字中用 <link="tooltip_id"><sprite=0></link> 包住 sprite
/// </summary>
public class TestTmp : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI text_condition;

    private int currentLinkIndex = -1;

    SkillConfigData shopSkills;

    void Start()
    {
        shopSkills = SkillDatabase.GetSkillConfig(9);
        text.text = shopSkills.effectText;
        text_condition.text = shopSkills.conditionText;
    }
    void SkillConditionLocalization(ref SkillConfigData _skill)
    {
        switch (_skill.requirementType)
        {
            case SkillRequirementType.SpecificDices:
                //不動作，直接顯示在說明裡面
                break;
            case SkillRequirementType.SameDices:
                _skill.conditionText = LanguageManager.GetFormat("T_Skill_SameDices", _skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.DiceSum:
                //不動作，直接顯示在說明裡面
                break;
            case SkillRequirementType.SpecificDicesWithRepeat:
                if (_skill.needDicesData[0] == 1)
                    _skill.conditionText = LanguageManager.GetFormat("T_Skill_SpecificDicesWithRepeat_low", _skill.GetNeedDiceNum());
                else
                    _skill.conditionText = LanguageManager.GetFormat("T_Skill_SpecificDicesWithRepeat_high", _skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.ConsecutiveDices:
                _skill.conditionText = LanguageManager.GetFormat("T_Skill_ConsecutiveDices", _skill.GetNeedDiceNum());
                break;
            case SkillRequirementType.AnyDices:
                _skill.conditionText = LanguageManager.GetFormat("T_Skill_AnyDices", _skill.GetNeedDiceNum());
                break;
        }
    }
    void SkillEffectLocalization(ref SkillConfigData _skill)
    {
        switch (_skill.skillType)
        {
            case SkillType.Attack:
                _skill.effectText = LanguageManager.GetFormat("T_Skill_SkillType_atk", _skill.skillValue);
                break;
            case SkillType.Heal:
                _skill.effectText = LanguageManager.GetFormat("T_Skill_SkillType_heal", _skill.skillValue);
                break;
            case SkillType.Buff:
                break;
        }
        if (_skill.selfBuffs != null && _skill.selfBuffs.Length > 0)
        {
            foreach (var buff in _skill.selfBuffs)
            {
                //icon還沒畫完 先都用0的圖
                int buffIconID = 0;//buff.buffID
                _skill.effectText += "\n" + LanguageManager.GetFormat("T_Skill_selfBuff", buff.buffID, buffIconID, buff.duration);
            }
        }
        if (_skill.targetBuffs != null && _skill.targetBuffs.Length > 0)
        {
            foreach (var buff in _skill.targetBuffs)
            {
                //icon還沒畫完 先都用0的圖
                int buffIconID = 0;//buff.buffID
                _skill.effectText += "\n" + LanguageManager.GetFormat("T_Skill_targetBuff", buff.buffID, buffIconID, buff.duration);
            }
        }
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            text,
            eventData.position,
            eventData.enterEventCamera
        );

        if (linkIndex != -1)
        {
            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
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