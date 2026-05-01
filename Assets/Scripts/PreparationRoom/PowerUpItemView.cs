using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 單一升級項目的 UI 視圖
/// </summary>
public class PowerUpItemView : MonoBehaviour
{
    [Header("UI 元件")]
    [SerializeField] TMPro.TMP_Text txt_name;           // 屬性名稱
    [SerializeField] TMPro.TMP_Text txt_level;          // 目前等級
    [SerializeField] TMPro.TMP_Text txt_cost;           // 升級花費
    [SerializeField] Button btn_upgrade;      // 升級按鈕

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
        txt_name.text = LanguageManager.GetText(config.displayName);
        //  txt_description.text = LanguageManager.GetText(config.description);
        // txt_currentValue.text = currentValue.ToString();

        // 升級相關顯示
        if (isMaxLevel)
        {
            // 滿級
            //  txt_nextValue.text = "-";
            txt_cost.text = "-";//LanguageManager.GetText("T_PowerUp_MaxLevel");
            btn_upgrade.interactable = false;
            txt_level.text = "Max";
        }
        else
        {
            // 未滿級
            float nextIncrease = PowerUpManager.GetNextLevelIncrease(powerUpType);
            int cost = PowerUpManager.GetNextLevelCost(powerUpType);

            //  txt_nextValue.text = LanguageManager.GetText(config.displayName) + $"+{nextIncrease}";
            txt_cost.text = cost.ToString();
            btn_upgrade.interactable = canUpgrade;
            txt_level.text = $"Lv.{currentLevel + 1}";
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
