using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class HintBubble : MonoBehaviour
{
    [SerializeField] TMP_Text text_hint;
    [SerializeField] RectTransform hintPanel;
    [SerializeField] private float moveDuration = 0.5f;
    
    public void SetUp(string _hintText, Action onComplete = null)
    {
        text_hint.text = _hintText;
        hintPanel.anchoredPosition = new Vector2(450f, hintPanel.anchoredPosition.y);

        bool completionNotified = false;
        void NotifyCompletion()
        {
            if (completionNotified)
                return;

            completionNotified = true;
            onComplete?.Invoke();
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(hintPanel.DOAnchorPosX(-125f, moveDuration).SetEase(Ease.OutCubic));
        sequence.AppendInterval(1f);
        sequence.Append(hintPanel.DOAnchorPosX(450f, moveDuration).SetEase(Ease.InCubic));
        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
            NotifyCompletion();
        });
        sequence.OnKill(NotifyCompletion);
    }
}
