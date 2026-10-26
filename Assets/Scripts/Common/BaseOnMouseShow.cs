using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseOnMouseShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject showObj;
    [SerializeField] protected float showDelay = 0.5f;
    [SerializeField] protected float showDuration = 0.2f;
    [SerializeField] protected float hideDuration = 0.15f;
    [SerializeField] protected Ease showEase = Ease.OutBack;
    [SerializeField] protected Ease hideEase = Ease.InBack;
    private Coroutine showCoroutine;

    protected virtual void Awake()
    {
        if (showObj == null)
        {
            showObj = gameObject;
        }

        if (showObj != null)
        {
            showObj.transform.localScale = Vector3.zero;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        CancelPendingShow();

        if (showDelay <= 0f)
        {
            Show();
            return;
        }

        showCoroutine = StartCoroutine(ShowAfterDelay());
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        CancelPendingShow();
        Hide();
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);
        showCoroutine = null;
        Show();
    }

    private void CancelPendingShow()
    {
        if (showCoroutine == null)
        {
            return;
        }

        StopCoroutine(showCoroutine);
        showCoroutine = null;
    }

    public virtual void Show()
    {
        CancelPendingShow();

        if (showObj == null)
        {
            return;
        }

        showObj.transform.DOKill();
        showObj.transform.DOScale(1f, showDuration).SetEase(showEase);
    }

    public virtual void Hide()
    {
        if (showObj == null)
        {
            return;
        }

        showObj.transform.DOKill();
        showObj.transform.DOScale(0f, hideDuration).SetEase(hideEase);
    }

    protected virtual void OnDisable()
    {
        CancelPendingShow();

        if (showObj != null)
        {
            showObj.transform.DOKill();
        }
    }
}
