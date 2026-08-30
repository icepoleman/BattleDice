using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseOnMouseShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject showObj;
    [SerializeField] protected float showDuration = 0.2f;
    [SerializeField] protected float hideDuration = 0.15f;
    [SerializeField] protected Ease showEase = Ease.OutBack;
    [SerializeField] protected Ease hideEase = Ease.InBack;

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
        Show();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    public virtual void Show()
    {
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
        if (showObj != null)
        {
            showObj.transform.DOKill();
        }
    }
}
