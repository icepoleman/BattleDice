using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 單一升級項目的 UI 視圖
/// </summary>
public class PowerUpItemView : MonoBehaviour
{
    [Header("UI 元件")]
    [SerializeField] Text txt_name;           // 屬性名稱
    [SerializeField] Text txt_description;    // 屬性描述
    [SerializeField] Text txt_currentValue;   // 目前數值
    [SerializeField] Text txt_nextValue;      // 升級後數值（或增加量）
   // [SerializeField] Text txt_level;          // 目前等級
    [SerializeField] Text txt_cost;           // 升級花費
    [SerializeField] Button btn_upgrade;      // 升級按鈕
    [SerializeField] GameObject maxLevelIndicator; // 滿級標示
    
    private PowerUpType powerUpType;
    private PowerUpConfigData config;
    private Action<PowerUpType> onUpgradeCallback;
    
    /// <summary>
    /// 初始化設定
    /// </summary>
    public void Setup(PowerUpType type, PowerUpConfigData configData, Action<PowerUpType> onUpgrade)
    {
        powerUpType = type;
        config = configData;
        onUpgradeCallback = onUpgrade;
        
        // 設定按鈕事件
        if (btn_upgrade != null)
        {
            btn_upgrade.onClick.AddListener(OnUpgradeButtonClick);
        }
        
        Refresh();
    }
    
    /// <summary>
    /// 刷新顯示
    /// </summary>
    public void Refresh()
    {
        int currentLevel = PowerUpManager.GetCurrentLevel(powerUpType);
        float currentValue = PowerUpManager.GetCurrentValue(powerUpType);
        bool isMaxLevel = PowerUpManager.IsMaxLevel(powerUpType);
        bool canUpgrade = PowerUpManager.CanUpgrade(powerUpType);
        
        // 名稱
        if (txt_name != null)
        {
            //txt_name.text = LanguageManager.GetText(config.displayName);
        }
        
        // 描述
        if (txt_description != null)
        {
            txt_description.text = LanguageManager.GetText(config.description);
        }
        
        // 目前數值
        if (txt_currentValue != null)
        {
            txt_currentValue.text = currentValue.ToString();
        }
        
        // 等級
        /*if (txt_level != null)
        {
            txt_level.text = $"Lv.{currentLevel}/{config.maxLevel}";
        }*/
        
        // 升級相關顯示
        if (isMaxLevel)
        {
            // 滿級
            if (txt_nextValue != null) txt_nextValue.text = "-";
            if (txt_cost != null) txt_cost.text = LanguageManager.GetText("T_PowerUp_MaxLevel");
            if (btn_upgrade != null) btn_upgrade.interactable = false;
            if (maxLevelIndicator != null) maxLevelIndicator.SetActive(true);
        }
        else
        {
            // 未滿級
            float nextIncrease = PowerUpManager.GetNextLevelIncrease(powerUpType);
            int cost = PowerUpManager.GetNextLevelCost(powerUpType);
            
            if (txt_nextValue != null)
            {
                txt_nextValue.text = LanguageManager.GetText(config.displayName) + $"+{nextIncrease}";
            }
            
            if (txt_cost != null)
            {
                txt_cost.text = cost.ToString();
            }
            
            if (btn_upgrade != null)
            {
                btn_upgrade.interactable = canUpgrade;
            }
            
            if (maxLevelIndicator != null) maxLevelIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// 升級按鈕點擊
    /// </summary>
    void OnUpgradeButtonClick()
    {
        onUpgradeCallback?.Invoke(powerUpType);
    }
}
