using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using Spine;

public class SkillCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text_skillTitle;
    [SerializeField] Toggle tog_skillChoose;
    ISkillData skillData;

    [Header("選中效果")]
    [SerializeField] BaseMouseEffect baseMouseEffect;

    [Header("技能icon")]
    [SerializeField] Image img_skillIcon;
    [SerializeField] Material selectMaterial;

    bool hasSkillData = false;

    public bool HasSkillData => hasSkillData;
    public bool CanBeSelected => hasSkillData && tog_skillChoose != null && tog_skillChoose.interactable;

    public async void SetData(ISkillData _skillData, ToggleGroup _toggleGroup = null)
    {
        hasSkillData = _skillData != null;
        if (!hasSkillData)
        {
            img_skillIcon.transform.parent.gameObject.SetActive(false);
            text_skillTitle.text = string.Empty;
            tog_skillChoose.group = _toggleGroup;
            tog_skillChoose.isOn = false;
            img_skillIcon.material = null;
            return;
        }

        img_skillIcon.sprite = AtlasLoader.Instance.GetSkillSprite(_skillData.iconPath);
        img_skillIcon.transform.parent.gameObject.SetActive(true);
        img_skillIcon.SetNativeSize();
        skillData = _skillData;
        text_skillTitle.text = skillData.skillName;
        tog_skillChoose.group = _toggleGroup;
        tog_skillChoose.onValueChanged.RemoveAllListeners();
        tog_skillChoose.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
            {
                img_skillIcon.material = null;
                return;
            }
            img_skillIcon.material = selectMaterial;
            EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, _skillData);
        });
        if (tog_skillChoose.isOn)
        {
            img_skillIcon.material = selectMaterial;
        }
    }

    public void SelectByShortcut()
    {
        if (!CanBeSelected)
        {
            return;
        }

        tog_skillChoose.isOn = true;
    }

    //開關卡片使用interactable
    public void SetInteractable(bool _isInteractable)
    {
        tog_skillChoose.interactable = _isInteractable;
        baseMouseEffect.EffectEnabled = _isInteractable;
    }
}
