using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [SerializeField] Text txt_hint;
    [SerializeField] Button btn_confirm;
    [SerializeField] Button btn_cancel;
    public void SetUp(string message, System.Action onConfirm)
    {
        txt_hint.text = message;
        btn_confirm.onClick.AddListener(() => {onConfirm?.Invoke(); Destroy(this.gameObject); });
        btn_cancel.onClick.AddListener(() => { Destroy(this.gameObject); });
    }
}
