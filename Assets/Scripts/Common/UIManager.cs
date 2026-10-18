using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 通用UI管理器 - 提供常用UI彈窗的快捷生成方法
/// </summary>
public static class UIManager
{
    private static Transform canvasTransform;
    private static readonly Queue<HintBubbleRequest> hintBubbleQueue = new Queue<HintBubbleRequest>();
    private static bool isProcessingHintBubbles;

    private sealed class HintBubbleRequest
    {
        public string Message { get; }
        public Transform Parent { get; }
        public TaskCompletionSource<HintBubble> Completion { get; } = new TaskCompletionSource<HintBubble>();

        public HintBubbleRequest(string message, Transform parent)
        {
            Message = message;
            Parent = parent;
        }
    }

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
    public static Task<HintBubble> ShowHintBubble(string message, Transform parent = null)
    {
        HintBubbleRequest request = new HintBubbleRequest(message, parent);
        hintBubbleQueue.Enqueue(request);

        if (!isProcessingHintBubbles)
        {
            _ = ProcessHintBubbleQueue();
        }

        return request.Completion.Task;
    }

    private static async Task ProcessHintBubbleQueue()
    {
        isProcessingHintBubbles = true;

        while (hintBubbleQueue.Count > 0)
        {
            HintBubbleRequest request = hintBubbleQueue.Dequeue();
            try
            {
                GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "HintBubble" + ".prefab");
                if (prefab == null)
                {
                    Debug.LogError("無法載入 HintBubble prefab");
                    request.Completion.TrySetResult(null);
                    continue;
                }

                Transform targetParent = request.Parent ?? GetCanvas();
                GameObject hintObj = Object.Instantiate(prefab, targetParent);
                HintBubble hint = hintObj.GetComponent<HintBubble>();
                TaskCompletionSource<bool> playbackCompletion = new TaskCompletionSource<bool>();
                hint.SetUp(request.Message, () => playbackCompletion.TrySetResult(true));
                request.Completion.TrySetResult(hint);
                await playbackCompletion.Task;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                request.Completion.TrySetResult(null);
            }
        }

        isProcessingHintBubbles = false;
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
    public static async Task<GameObject> ShowCommonPanel(string panelAB, Transform parent = null)
    {
        GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + panelAB + ".prefab");
        if (prefab == null)
        {
            Debug.LogError($"無法載入 {panelAB} prefab");
            return null;
        }

        Transform targetParent = parent ?? GetCanvas();
        GameObject panelObj = Object.Instantiate(prefab, targetParent);
        AddRightClickCloser(panelObj);
        return panelObj;
    }
    public static async Task<GameObject> ShowPanel(string panelAB, Transform parent = null)
    {
        GameObject prefab = await AddressableManager.LoadAssetAsync<GameObject>(panelAB + ".prefab");
        if (prefab == null)
        {
            Debug.LogError($"無法載入 {panelAB} prefab");
            return null;
        }

        Transform targetParent = parent ?? GetCanvas();
        GameObject panelObj = Object.Instantiate(prefab, targetParent);
        AddRightClickCloser(panelObj);
        return panelObj;
    }

    /// <summary>
    /// 為彈窗添加右鍵關閉功能 //停用
    /// </summary>
    private static void AddRightClickCloser(GameObject obj)
    {
        if (obj.GetComponent<RightClickCloser>() == null)
        {
          //  obj.AddComponent<RightClickCloser>();
        }
    }
}
