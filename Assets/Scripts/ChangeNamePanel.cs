using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChangeNamePanel : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] Button btn_confirm;
    [SerializeField] RectTransform namePanel;
    private float moveUpDuration = 0.5f;
    private float moveDownDuration = 0.5f;
    private const int MaxNameLength = 6;
    private const float HiddenY = 1300f;
    private const float ShownY = 230f;
    private bool isClosing;

    void Start()
    {
        inputField.characterLimit = MaxNameLength;
        btn_confirm.onClick.AddListener(OnConfirmClicked);
        namePanel.anchoredPosition = new Vector2(namePanel.anchoredPosition.x, HiddenY);
        namePanel.DOAnchorPosY(ShownY, moveUpDuration).SetEase(Ease.OutBack);
    }

    void OnConfirmClicked()
    {
        string newName = inputField.text.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            UIManager.ShowHintBubble(LanguageManager.GetFormat("T_SetNameNo"));
            return;
        }

        if (isClosing)
        {
            return;
        }

        isClosing = true;
        btn_confirm.interactable = false;
        GameDataManager.PlayerName = newName;
        namePanel.DOKill();
        namePanel.DOAnchorPosY(HiddenY, moveDownDuration)
            .SetEase(Ease.InBack, 0.5f)
            .OnComplete(() =>
            {
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
            });
    }
}
