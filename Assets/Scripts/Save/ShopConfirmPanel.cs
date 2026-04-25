using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopConfirmPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_hint;
    [SerializeField] TextMeshProUGUI txt_itemName;
    [SerializeField] TextMeshProUGUI txt_price;
    [SerializeField] Button btn_confirm;
    [SerializeField] Button btn_cancel;
    [SerializeField] Button btn_cancel_back;
    public void SetUp(string _hint, string _itemName, string _price, System.Action onConfirm, System.Action onCancel = null)
    {
        txt_hint.text = _hint;
        txt_itemName.text = _itemName;
        txt_price.text = _price;
        btn_confirm.onClick.AddListener(() => {onConfirm?.Invoke(); Destroy(this.gameObject); });
        btn_cancel.onClick.AddListener(() => { onCancel?.Invoke(); Destroy(this.gameObject); });
        btn_cancel_back.onClick.AddListener(() => { onCancel?.Invoke(); Destroy(this.gameObject); });
    }
}
