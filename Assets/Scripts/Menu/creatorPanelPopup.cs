using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class creatorPanelPopup : MonoBehaviour
{
    [SerializeField] private RectTransform transPanel;
    [SerializeField] private Button btn_close;
    [SerializeField] private float panelMoveDuration = 0.35f;

    private void Start()
    {
        btn_close.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel()
    {
        transPanel.gameObject.SetActive(true);
        transPanel.DOComplete();
        transPanel.DOAnchorPosY(0f, panelMoveDuration).SetEase(Ease.OutCubic);
    }

    private void ClosePanel()
    {
        transPanel.DOComplete();
        transPanel.DOAnchorPosY(-1500f, panelMoveDuration).SetEase(Ease.InCubic)
            .OnComplete(() => transPanel.gameObject.SetActive(false));
    }
}
