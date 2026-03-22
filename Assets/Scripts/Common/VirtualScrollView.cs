using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

///*範例
      // 1. 初始化
   /*    virtualScrollView.Init((cell, index) =>
        {
            // index 從 0 開始
            cell.GetComponentInChildren<Text>().text = $"Item {index}";
        });

        // 2. 設置數量（100筆資料）
        virtualScrollView.SetItemCount(100);

        // 3. 刷新所有可見項目
        virtualScrollView.RefreshAll();
        */

/// <summary>
/// 虛擬化 ScrollView - 只生成可視範圍內的 Cell，自動回收重複利用
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class VirtualScrollView : MonoBehaviour
{
    public enum Direction { Vertical, Horizontal }

    [Header("設置")]
    [SerializeField] private Direction direction = Direction.Vertical;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float spacing = 10f;        // 間距
    [SerializeField] private int bufferCount = 2;        // 額外緩衝數量

    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;

    private int totalCount;
    private int visibleCount;
    private int startIndex;

    private List<GameObject> activeItems = new List<GameObject>();
    private Stack<GameObject> pool = new Stack<GameObject>();

    private Action<GameObject, int> onItemUpdate;

    private bool isInitialized;

    private float cellWidth = 0f;  
    private float cellHeight = 0f;    

    // 取得滾動方向的 Cell 尺寸
    private float CellSize => direction == Direction.Vertical ? cellHeight : cellWidth;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="onUpdate">Cell 更新回調 (cell, index從0開始)</param>
    public void Init(Action<GameObject, int> onUpdate)
    {
        onItemUpdate = onUpdate;

        if (isInitialized) return;

        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport != null ? scrollRect.viewport : GetComponent<RectTransform>();
        content = scrollRect.content;

        // 取得 Cell 模板
        if (cellPrefab == null && content.childCount > 0)
        {
            cellPrefab = content.GetChild(0).gameObject;
            cellPrefab.SetActive(false);
        }

        // 如果沒設置尺寸，從 prefab 取得
        RectTransform prefabRect = cellPrefab.GetComponent<RectTransform>();
        cellWidth = prefabRect.rect.width;
        cellHeight = prefabRect.rect.height;

        // 設置 Content 錨點
        if (direction == Direction.Vertical)
        {
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            content.anchorMin = new Vector2(0, 0);
            content.anchorMax = new Vector2(0, 1);
            content.pivot = new Vector2(0, 0.5f);
        }

        // 計算可見數量
        float viewportSize = direction == Direction.Vertical ? viewport.rect.height : viewport.rect.width;
        visibleCount = Mathf.CeilToInt(viewportSize / (CellSize + spacing)) + bufferCount * 2;

        // 監聯滾動
        scrollRect.onValueChanged.AddListener(OnScroll);

        isInitialized = true;
    }

    /// <summary>
    /// 設置數據數量並顯示
    /// </summary>
    public void SetItemCount(int count, bool resetPosition = true)
    {
        totalCount = count;

        // 回收所有 Cell
        RecycleAll();

        // 設置 Content 大小
        float contentSize = count * CellSize + (count - 1) * spacing;
        if (direction == Direction.Vertical)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, contentSize);
        }
        else
        {
            content.sizeDelta = new Vector2(contentSize, content.sizeDelta.y);
        }

        // 重置位置
        if (resetPosition)
        {
            content.anchoredPosition = Vector2.zero;
            startIndex = 0;
        }
        else
        {
            startIndex = GetCurrentStartIndex();
        }

        // 生成可見的 Cell
        RefreshVisibleItems();
    }

    /// <summary>
    /// 刷新所有可見項目
    /// </summary>
    public void RefreshAll()
    {
        foreach (var item in activeItems)
        {
            int index = int.Parse(item.name);
            onItemUpdate?.Invoke(item, index);
        }
    }

    /// <summary>
    /// 刷新指定項目（如果可見）
    /// </summary>
    public void RefreshItem(int index)
    {
        foreach (var item in activeItems)
        {
            if (int.Parse(item.name) == index)
            {
                onItemUpdate?.Invoke(item, index);
                break;
            }
        }
    }

    /// <summary>
    /// 滾動到指定索引
    /// </summary>
    public void ScrollToIndex(int index)
    {
        float pos = index * (CellSize + spacing);
        if (direction == Direction.Vertical)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, pos);
        }
        else
        {
            content.anchoredPosition = new Vector2(-pos, content.anchoredPosition.y);
        }
    }

    private void OnScroll(Vector2 _)
    {
        int newStartIndex = GetCurrentStartIndex();

        if (newStartIndex != startIndex)
        {
            startIndex = newStartIndex;
            RefreshVisibleItems();
        }
    }

    private int GetCurrentStartIndex()
    {
        float scrollPos = direction == Direction.Vertical
            ? content.anchoredPosition.y
            : -content.anchoredPosition.x;

        int index = Mathf.FloorToInt(scrollPos / (CellSize + spacing)) - bufferCount;
        return Mathf.Max(0, index);
    }

    private void RefreshVisibleItems()
    {
        // 計算需要顯示的範圍
        int endIndex = Mathf.Min(startIndex + visibleCount, totalCount);

        // 建立需要顯示的 index 集合
        HashSet<int> neededIndices = new HashSet<int>();
        for (int i = startIndex; i < endIndex; i++)
        {
            neededIndices.Add(i);
        }

        // 回收不需要的 Cell
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            int itemIndex = int.Parse(activeItems[i].name);
            if (!neededIndices.Contains(itemIndex))
            {
                ReturnToPool(activeItems[i]);
                activeItems.RemoveAt(i);
            }
            else
            {
                neededIndices.Remove(itemIndex); // 已存在，不需要重新生成
            }
        }

        // 生成新的 Cell
        foreach (int index in neededIndices)
        {
            GameObject item = GetFromPool();
            item.name = index.ToString();

            // 設置位置
            RectTransform rect = item.GetComponent<RectTransform>();
            float pos = index * (CellSize + spacing);

            if (direction == Direction.Vertical)
            {
                rect.anchoredPosition = new Vector2(0, -pos - cellHeight * 0.5f);
            }
            else
            {
                rect.anchoredPosition = new Vector2(pos + cellWidth * 0.5f, 0);
            }

            item.SetActive(true);
            activeItems.Add(item);

            // 回調更新
            onItemUpdate?.Invoke(item, index);
        }
    }

    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }

        GameObject item = Instantiate(cellPrefab, content);
        item.transform.localScale = Vector3.one;

        // 設置 Cell 錨點與尺寸
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        if (direction == Direction.Vertical)
        {
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(cellWidth, cellHeight);
        }
        else
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(cellWidth, cellHeight);
        }

        return item;
    }

    private void ReturnToPool(GameObject item)
    {
        item.SetActive(false);
        pool.Push(item);
    }

    private void RecycleAll()
    {
        foreach (var item in activeItems)
        {
            ReturnToPool(item);
        }
        activeItems.Clear();
    }

    private void OnDestroy()
    {
        onItemUpdate = null;
    }

    /// <summary>
    /// 當前啟用的 Cell 數量（用於調試）
    /// </summary>
    public int ActiveCount => activeItems.Count;

    /// <summary>
    /// 總數量
    /// </summary>
    public int TotalCount => totalCount;
}
