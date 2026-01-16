using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PowerUpPanel : MonoBehaviour
{
    [Header("升級項目（依序對應 MaxBlood, DiceCount, KeepDiceCount, MaxRollCount）")]
    [SerializeField] PowerUpItemView item_MaxBlood;
    [SerializeField] PowerUpItemView item_DiceCount;
    [SerializeField] PowerUpItemView item_KeepDiceCount;
    [SerializeField] PowerUpItemView item_MaxRollCount;
    [SerializeField] Button btn_close;
    
    [Header("玩家資源顯示")]
    [SerializeField] Text txt_gearCount;
    
    private List<PowerUpItemView> itemViews = new List<PowerUpItemView>();
    
    void Start()
    {
        InitializeItems();
        UpdateGearDisplay();
        btn_close.onClick.AddListener(() => 
        {
            Destroy(gameObject);
        });
    }
    
    /// <summary>
    /// 初始化升級項目列表
    /// </summary>
    void InitializeItems()
    {
        // 設定每個項目
        SetupItem(item_MaxBlood, PowerUpType.MaxBlood);
        SetupItem(item_DiceCount, PowerUpType.DiceCount);
        SetupItem(item_KeepDiceCount, PowerUpType.KeepDiceCount);
        SetupItem(item_MaxRollCount, PowerUpType.MaxRollCount);
    }
    
    /// <summary>
    /// 設定單一升級項目
    /// </summary>
    void SetupItem(PowerUpItemView itemView, PowerUpType type)
    {
        if (itemView == null) return;
        
        var config = PowerUpDatabase.GetConfig(type);
        itemView.Setup(type, config, OnUpgradeClicked);
        itemViews.Add(itemView);
    }
    
    /// <summary>
    /// 升級按鈕點擊回調
    /// </summary>
    void OnUpgradeClicked(PowerUpType type)
    {
        if (PowerUpManager.TryUpgrade(type))
        {
            // 升級成功，更新所有顯示
            RefreshAllItems();
            UpdateGearDisplay();
        }
    }
    
    /// <summary>
    /// 刷新所有升級項目顯示
    /// </summary>
    public void RefreshAllItems()
    {
        foreach (var itemView in itemViews)
        {
            itemView.Refresh();
        }
    }
    
    /// <summary>
    /// 更新齒輪數量顯示
    /// </summary>
    void UpdateGearDisplay()
    {
        if (txt_gearCount != null)
        {
            txt_gearCount.text = GameDataManager.Gear.ToString();
        }
    }
}
