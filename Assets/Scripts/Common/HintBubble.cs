using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HintBubble : MonoBehaviour
{
    [SerializeField] Text text_hint;
    [SerializeField] Image bg_image;
    float displayDuration = 2f;    // 顯示時間
    float fadeDuration = 1f;     // 淡出時間
    [SerializeField] float fadeMoveUpDistance = 40f;

    RectTransform rectTransform;
    Vector2 originalAnchoredPosition;

    void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }
    }
    
    public void SetUp(string _hintText)
    {
        bg_image.DOKill();
        text_hint.DOKill();
        rectTransform?.DOKill();

        text_hint.text = _hintText;
        
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        // 重置透明度
        SetAlpha(1f);
        
        // 延遲後淡出
        DOVirtual.DelayedCall(displayDuration, FadeOut);
    }
    
    void FadeOut()
    {
        // 圖片和文字同時淡出
        bg_image.DOFade(0f, fadeDuration);
        text_hint.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            Destroy(gameObject);
        });

        if (rectTransform != null)
        {
            rectTransform.DOAnchorPosY(originalAnchoredPosition.y + fadeMoveUpDistance, fadeDuration).SetEase(Ease.OutQuad);
        }
    }
    
    void SetAlpha(float alpha)
    {
        Color bgColor = bg_image.color;
        bgColor.a = alpha;
        bg_image.color = bgColor;
        
        Color textColor = text_hint.color;
        textColor.a = alpha;
        text_hint.color = textColor;
    }
}
