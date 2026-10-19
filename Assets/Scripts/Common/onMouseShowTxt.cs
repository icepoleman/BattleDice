using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class onMouseShowTxt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform rect_txt;
    [SerializeField] private bool expandLeft;
    private TextMeshProUGUI showTxt;

    public float ShownOffset = 5f;
    private const float Duration = 0.5f;
    private const float HideEarlyClose = 0.2f;

    void Start()
    {
        showTxt = rect_txt.GetComponent<TextMeshProUGUI>();
        OnPointerExit(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rect_txt == null)
            return;

        rect_txt.DOKill();
        showTxt?.DOKill();
        rect_txt.gameObject.SetActive(true);
        SetTextAlpha(0f);
        rect_txt.DOAnchorPosX(ShownX, Duration).SetEase(Ease.OutCubic).SetLink(rect_txt.gameObject);
        showTxt?.DOFade(1f, Duration).SetEase(Ease.OutCubic).SetLink(rect_txt.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rect_txt == null)
            return;

        rect_txt.DOKill();
        showTxt?.DOKill();
        float hiddenX = ShownX + (expandLeft ? rect_txt.rect.width * 0.5f : -rect_txt.rect.width * 0.5f);
        float fadeDuration = Duration - HideEarlyClose;
        Tween hideTween = rect_txt.DOAnchorPosX(hiddenX, Duration).SetEase(Ease.OutCubic);
        showTxt?.DOFade(0f, fadeDuration).SetEase(Ease.OutCubic).SetLink(rect_txt.gameObject);
        hideTween.OnUpdate(() =>
        {
            if (rect_txt != null && hideTween.Elapsed() >= fadeDuration)
            {
                showTxt?.DOKill();
                SetTextAlpha(0f);
                rect_txt.gameObject.SetActive(false);
            }
        }).SetLink(rect_txt.gameObject);
    }

    private void SetTextAlpha(float alpha)
    {
        if (showTxt == null)
            return;

        Color color = showTxt.color;
        color.a = alpha;
        showTxt.color = color;
    }

    private float ShownX => expandLeft ? -ShownOffset : ShownOffset;
}
