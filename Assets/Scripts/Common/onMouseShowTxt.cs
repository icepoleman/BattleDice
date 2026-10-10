using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class onMouseShowTxt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform rect_txt;
    [SerializeField] private bool expandLeft;

    public float ShownOffset = 5f;
    private const float Duration = 0.5f;
    private const float HideEarlyClose = 0.2f;

    void Start()
    {
        OnPointerExit(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rect_txt == null)
            return;

        rect_txt.DOKill();
        rect_txt.gameObject.SetActive(true);
        rect_txt.DOAnchorPosX(ShownX, Duration).SetEase(Ease.OutCubic).SetLink(rect_txt.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rect_txt == null)
            return;

        rect_txt.DOKill();
        float hiddenX = ShownX + (expandLeft ? rect_txt.rect.width * 0.5f : -rect_txt.rect.width * 0.5f);
        Tween hideTween = rect_txt.DOAnchorPosX(hiddenX, Duration).SetEase(Ease.OutCubic);
        hideTween.OnUpdate(() =>
        {
            if (rect_txt != null && hideTween.Elapsed() >= Duration - HideEarlyClose)
                rect_txt.gameObject.SetActive(false);
        }).SetLink(rect_txt.gameObject);
    }

    private float ShownX => expandLeft ? -ShownOffset : ShownOffset;
}
