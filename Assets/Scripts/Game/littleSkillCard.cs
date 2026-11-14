using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class littleSkillCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ISkillData skillData;
    private const float HOVER_SHOW_DELAY = 0.3f;
    private Coroutine hoverShowCoroutine;
    private bool hasDispatchedThisHover;
    private Canvas rootCanvas;
    [SerializeField] Material chooseMt;
    [SerializeField] Image img_skillIcon;

    public void SetData(ISkillData _skillData)
    {
        skillData = _skillData;
        img_skillIcon.sprite = AtlasLoader.Instance.GetSkillSprite(_skillData.iconPath);
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
        img_skillIcon.material = isOn ? chooseMt : null;
    }

    private void OnDisable()
    {
        if (hasDispatchedThisHover)
        {
            EventCenter.Dispatch(GameEvent.EVENT_HIDE_SKILL_DETAIL);
        }

        CancelHoverShow();
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
        EventCenter.Dispatch(GameEvent.EVENT_SHOW_SKILL_DETAIL, GetSkillID(), screenPos);
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
