using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 通用UI管理器 - 提供常用UI彈窗的快捷生成方法
/// </summary>
public static class CommonUIManager
{
    private static Transform canvasTransform;

    /// <summary>
    /// 設定Canvas（可選，若不設定會自動找Canvas）
    /// </summary>
    public static void SetCanvas(Transform canvas)
    {
        canvasTransform = canvas;
    }

    /// <summary>
    /// 取得Canvas
    /// </summary>
    private static Transform GetCanvas()
    {
        if (canvasTransform == null)
        {
            // 優先尋找 Canvas_Popup
            GameObject popupCanvas = GameObject.Find("Canvas_Popup");
            if (popupCanvas != null)
            {
                canvasTransform = popupCanvas.transform;
            }
            else
            {
                // 找不到 Canvas_Popup，使用預設 Canvas
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    canvasTransform = canvas.transform;
                }
            }
        }
        return canvasTransform;
    }

    /// <summary>
    /// 顯示提示泡泡
    /// </summary>
    /// <param name="message">提示文字</param>
    /// <param name="parent">父物件（可選，預設為Canvas）</param>
    public static async Task<HintBubble> ShowHintBubble(string message, Transform parent = null)
    {
        GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "HintBubble" + ".prefab");
        if (prefab == null)
        {
            Debug.LogError("無法載入 HintBubble prefab");
            return null;
        }

        Transform targetParent = parent ?? GetCanvas();
        GameObject hintObj = Object.Instantiate(prefab, targetParent);
        HintBubble hint = hintObj.GetComponent<HintBubble>();
        hint.SetUp(message);
        return hint;
    }

    /// <summary>
    /// 顯示確認彈窗
    /// </summary>
    /// <param name="message">提示文字</param>
    /// <param name="onConfirm">確認回調</param>
    /// <param name="onCancel">取消回調（可選）</param>
    /// <param name="parent">父物件（可選，預設為Canvas）</param>
    public static async Task<ConfirmPanel> ShowConfirmPanel(string message, Action onConfirm, Action onCancel = null, Transform parent = null)
    {
        GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "ConfirmPanel" + ".prefab");
        if (prefab == null)
        {
            Debug.LogError("無法載入 ConfirmPanel prefab");
            return null;
        }

        Transform targetParent = parent ?? GetCanvas();
        GameObject panelObj = Object.Instantiate(prefab, targetParent);
        ConfirmPanel panel = panelObj.GetComponent<ConfirmPanel>();
        panel.SetUp(message, onConfirm, onCancel);
        return panel;
    }
    public static async Task ShowPanel(string panelAB, Transform parent = null)
    {
        GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + panelAB + ".prefab");
        if (prefab == null)
        {
            Debug.LogError($"無法載入 {panelAB} prefab");
        }

        Transform targetParent = parent ?? GetCanvas();
        GameObject hintObj = Object.Instantiate(prefab, targetParent);
    }
}
