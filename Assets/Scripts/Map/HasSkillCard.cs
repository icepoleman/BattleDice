using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

public class HasSkillCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool _isChosen;
    public bool isChosen
    {
        get => _isChosen;
        set
        {
            _isChosen = value;
            img_choose.gameObject.SetActive(value);
        }
    }

    public int SkillID => skillData.skillID;

    [SerializeField] Button btn_card;
    [SerializeField] Image img_skillIcon;
    [SerializeField] Image img_choose;
    private SkillConfigData skillData;
    private const float HOVER_SHOW_DELAY = 0.3f;
    private Coroutine hoverShowCoroutine;
    private bool hasDispatchedThisHover;
    private Canvas rootCanvas;

    public void SetData(SkillConfigData _skillData, Action<HasSkillCard> onCardClicked)
    {
        if(onCardClicked == null)
        {
            btn_card.gameObject.SetActive(false);
        }
        btn_card.onClick.AddListener(() => onCardClicked?.Invoke(this));
        img_skillIcon.sprite = AtlasLoader.Instance.GetSkillSprite(_skillData.iconPath);
        img_skillIcon.SetNativeSize();
        skillData = _skillData;
    }

    void OnEnable()
    {
        EventCenter.AddListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_UNCHOOSE_SKILL, OnUnchooseSkill);
    }
    void OnDisable()
    {
        CancelHoverShow();
    }
    void OnUnchooseSkill(object[] args)
    {
        int skillID = (int)args[0];
        if (skillID == this.SkillID)
        {
            isChosen = false;
            img_choose.gameObject.SetActive(false);
            Debug.Log("Unchoose skill: " + skillID);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CancelHoverShow();
        hasDispatchedThisHover = false;
        hoverShowCoroutine = StartCoroutine(ShowSkillDetailAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelHoverShow();
        EventCenter.Dispatch(GameEvent.EVENT_HIDE_SKILL_DETAIL);
    }

    private IEnumerator ShowSkillDetailAfterDelay()
    {
        yield return new WaitForSeconds(HOVER_SHOW_DELAY);

        if (hasDispatchedThisHover)
        {
            yield break;
        }

        hasDispatchedThisHover = true;
        Vector2 screenPos = GetCardScreenPosition();
        EventCenter.Dispatch(GameEvent.EVENT_SHOW_SKILL_DETAIL, SkillID, screenPos);
    }

    private Vector2 GetCardScreenPosition()
    {
        RectTransform cardRect = transform as RectTransform;

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        Camera uiCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        }

        Vector3 worldPos = cardRect != null ? cardRect.TransformPoint(cardRect.rect.center) : transform.position;
        return RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);
    }

    private void CancelHoverShow()
    {
        if (hoverShowCoroutine != null)
        {
            StopCoroutine(hoverShowCoroutine);
            hoverShowCoroutine = null;
        }

        hasDispatchedThisHover = false;
    }
}
