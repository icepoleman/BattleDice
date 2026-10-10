using UnityEngine;
using UnityEngine.UI;
public class ChangeNamePanel : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] Button btn_confirm;
    private const int MaxNameLength = 6;

    void Start()
    {
        inputField.characterLimit = MaxNameLength;
        btn_confirm.onClick.AddListener(OnConfirmClicked);
    }

    void OnConfirmClicked()
    {
        string newName = inputField.text.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            UIManager.ShowHintBubble(LanguageManager.GetFormat("T_SetNameNo"));
            return;
        }

        GameDataManager.PlayerName = newName;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
        Destroy(gameObject);
    }
}
