using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using DG.Tweening;

public class SkillDetailHint : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Vector2 skillDetailOffset = new Vector2(0f, 70f);
    [SerializeField] private TextMeshProUGUI text_infoTitle;
    [SerializeField] private TextMeshProUGUI text_skillEffect;
    [SerializeField] private GameObject buffDetailPrefab;
    [SerializeField] private Transform buffDetailRoot;

    private void Awake()
    {
        if (uiCanvas == null)
        {
            uiCanvas = GetComponentInParent<Canvas>();
        }

        EventCenter.AddListener(GameEvent.EVENT_SHOW_SKILL_DETAIL, OnShowSkillDetail);
        EventCenter.AddListener(GameEvent.EVENT_HIDE_SKILL_DETAIL, OnHideSkillDetail);
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_SHOW_SKILL_DETAIL, OnShowSkillDetail);
        EventCenter.RemoveListener(GameEvent.EVENT_HIDE_SKILL_DETAIL, OnHideSkillDetail);
    }

    public void SetData(SkillConfigData _skillData)
    {
        transform.DOKill();
        transform.localScale = Vector3.one * 0.7f;

        gameObject.SetActive(true);
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

        text_infoTitle.text = _skillData.skillName;
        text_skillEffect.text = "";
        text_skillEffect.text += LanguageManager.GetText("T_skill_condition_title");
        text_skillEffect.text += "\n" + _skillData.conditionText;
        text_skillEffect.text += "\n" + LanguageManager.GetText("T_skill_effect_title");
        text_skillEffect.text += "\n" + _skillData.effectText;

        ClearBuffDetails();

        var seenBuffs = new HashSet<int>();
        var allBuffs = (_skillData.selfBuffs ?? Array.Empty<BuffSeed>())
            .Concat(_skillData.targetBuffs ?? Array.Empty<BuffSeed>());

        foreach (var buff in allBuffs)
        {
            if (seenBuffs.Add(buff.buffID))
            {
                SpawnBuffDetail(buff.buffID);
            }
        }

        //img_skillIcon.sprite = AtlasLoader.Instance.GetSkillSprite(_skillData.iconPath);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnShowSkillDetail(object[] args)
    {
        int skillID = (int)args[0];
        Vector2 skillScreenPos = args[1] is Vector2 v2
            ? v2
            : (Vector2)(Vector3)args[1];
        SkillConfigData skillData = SkillDatabase.GetSkillConfig(skillID);
        MoveSkillDetailHint(skillScreenPos);
        SetData(skillData);
    }

    private void OnHideSkillDetail(object[] args)
    {
        Hide();
    }

    private void MoveSkillDetailHint(Vector2 screenPos)
    {
        RectTransform hintRect = transform as RectTransform;
        if (hintRect == null)
        {
            transform.position = screenPos;
            return;
        }

        RectTransform parentRect = hintRect.parent as RectTransform;
        if (parentRect == null)
        {
            transform.position = screenPos;
            return;
        }

        Camera uiCamera = null;
        if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = uiCanvas.worldCamera != null ? uiCanvas.worldCamera : Camera.main;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out Vector2 localPoint))
        {
            hintRect.anchoredPosition = localPoint + skillDetailOffset;
        }
    }

    private void SpawnBuffDetail(int buffID)
    {
        if (buffDetailPrefab == null || buffDetailRoot == null)
        {
            return;
        }

        GameObject buffObject = Instantiate(buffDetailPrefab, buffDetailRoot);
        BuffDetailHint buffDetailHint = buffObject.GetComponent<BuffDetailHint>();
        if (buffDetailHint != null)
        {
            buffDetailHint.SetData(buffID);
        }
    }

    private void ClearBuffDetails()
    {
        if (buffDetailRoot == null)
        {
            return;
        }

        foreach (Transform child in buffDetailRoot)
        {
            Destroy(child.gameObject);
        }
    }
}
