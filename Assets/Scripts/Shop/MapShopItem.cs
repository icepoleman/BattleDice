using System;
using UnityEngine;
using UnityEngine.UI;

public class MapShopItem : MonoBehaviour
{
    [SerializeField]Text txt_itemName;
    [SerializeField]Text txt_info;
    [SerializeField]Button btn_buy;
    [SerializeField]GameObject obj_soldOut;
    public void SetUp(string itemName, string info, ItemType itemType, Action onBuy)
    {
        txt_itemName.text = itemName;
        txt_info.text = info;
        //txt_priceText.text = price.ToString();
        btn_buy.onClick.RemoveAllListeners();
        btn_buy.onClick.AddListener(() => onBuy?.Invoke());
    }
    
}
