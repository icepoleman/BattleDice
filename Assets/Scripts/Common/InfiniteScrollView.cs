using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

/// <summary>
/// 無限滾動列表 - 只渲染可視範圍內的 Cell，支持對象池回收
/// </summary>
public class InfiniteScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Direction { Horizontal, Vertical }

    [Header("基本設置")]
    [SerializeField] private Direction direction = Direction.Vertical;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int columnCount = 1; // 列數（垂直）或行數（水平）
    [SerializeField] private float spacing = 10f;
    [SerializeField] private RectOffset padding;

    [Header("箭頭指示器")]
    [SerializeField] private GameObject arrowStart;
    [SerializeField] private GameObject arrowEnd;

    // 回調
    private Action<GameObject, int> onCellUpdate;
    private Action onPullRefresh;

    // 組件
    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;

    // Cell 資訊
    private float cellWidth;
    private float cellHeight;
    private int totalCount;

    // Cell 狀態追蹤
    private class CellData
    {
        public Vector2 position;
        public GameObject instance;
    }
    private CellData[] cells;

    // 對象池
    private Stack<GameObject> pool = new Stack<GameObject>();

    private bool isInitialized;

    #region 初始化
    /// <summary>
    /// 初始化滾動列表
    /// </summary>
    /// <param name="onUpdate">Cell 更新回調 (cell物件, index從1開始)</param>
    public void Init(Action<GameObject, int> onUpdate)
    {
        onCellUpdate = onUpdate;

        if (isInitialized) return;

        scrollRect = GetComponent<ScrollRect>();
        viewport = GetComponent<RectTransform>();
        content = scrollRect.content;

        // 設置 Cell 模板
        if (cellPrefab == null)
            cellPrefab = content.GetChild(0).gameObject;

        RectTransform cellRect = cellPrefab.GetComponent<RectTransform>();
        cellWidth = cellRect.rect.width;
        cellHeight = cellRect.rect.height;

        // 設置錨點
        SetupAnchors();

        // 隱藏模板並放入對象池
        cellPrefab.SetActive(false);
        pool.Push(cellPrefab);

        // 監聽滾動事件
        scrollRect.onValueChanged.AddListener(OnScroll);

        isInitialized = true;
    }

    /// <summary>
    /// 設置上拉刷新回調
    /// </summary>
    public void SetPullRefreshCallback(Action callback)
    {
        onPullRefresh = callback;
    }

    private void SetupAnchors()
    {
        // Content 錨點設置為左上角
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
        content.pivot = new Vector2(0, 1);
    }
    #endregion

    #region 顯示列表
    /// <summary>
    /// 顯示列表
    /// </summary>
    /// <param name="count">項目數量</param>
    /// <param name="resetPosition">是否重置滾動位置</param>
    public void ShowList(int count, bool resetPosition = true)
    {
        // 回收所有現有 Cell
        RecycleAllCells();

        totalCount = count;
        cells = new CellData[count];

        // 計算 Content 大小
        ResizeContent();

        // 重置滾動位置
        if (resetPosition)
            content.anchoredPosition = Vector2.zero;

        // 計算每個 Cell 的位置並顯示可視範圍內的
        for (int i = 0; i < count; i++)
        {
            cells[i] = new CellData { position = CalculateCellPosition(i) };

            if (IsInViewport(cells[i].position))
            {
                ShowCell(i);
            }
        }

        UpdateArrows();
    }

    /// <summary>
    /// 刷新所有可視的 Cell
    /// </summary>
    public void RefreshVisibleCells()
    {
        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].instance != null && IsInViewport(cells[i].position))
            {
                onCellUpdate?.Invoke(cells[i].instance, i + 1);
            }
        }
    }

    /// <summary>
    /// 刷新指定 Cell
    /// </summary>
    public void RefreshCell(int index)
    {
        if (cells == null || index < 0 || index >= cells.Length) return;

        if (cells[index].instance != null)
        {
            onCellUpdate?.Invoke(cells[index].instance, index + 1);
        }
    }
    #endregion

    #region 位置計算
    private void ResizeContent()
    {
        int rowCount = Mathf.CeilToInt((float)totalCount / columnCount);

        if (direction == Direction.Vertical)
        {
            float height = padding.top + padding.bottom + rowCount * cellHeight + (rowCount - 1) * spacing;
            height = Mathf.Max(height, viewport.rect.height);
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
        }
        else
        {
            float width = padding.left + padding.right + rowCount * cellWidth + (rowCount - 1) * spacing;
            width = Mathf.Max(width, viewport.rect.width);
            content.sizeDelta = new Vector2(width, content.sizeDelta.y);
        }
    }

    private Vector2 CalculateCellPosition(int index)
    {
        int row = index / columnCount;
        int col = index % columnCount;

        if (direction == Direction.Vertical)
        {
            float x = padding.left + col * (cellWidth + spacing) + cellWidth * 0.5f;
            float y = -(padding.top + row * (cellHeight + spacing) + cellHeight * 0.5f);
            return new Vector2(x, y);
        }
        else
        {
            float x = padding.left + row * (cellWidth + spacing);
            float y = -(padding.top + col * (cellHeight + spacing));
            return new Vector2(x, y);
        }
    }

    private bool IsInViewport(Vector2 cellPos)
    {
        Vector2 contentPos = content.anchoredPosition;

        if (direction == Direction.Vertical)
        {
            float viewTop = -contentPos.y;
            float viewBottom = viewTop - viewport.rect.height;
            float cellY = cellPos.y;

            return cellY + cellHeight * 0.5f > viewBottom - cellHeight &&
                   cellY - cellHeight * 0.5f < viewTop + cellHeight;
        }
        else
        {
            float viewLeft = -contentPos.x;
            float viewRight = viewLeft + viewport.rect.width;
            float cellX = cellPos.x;

            return cellX + cellWidth * 0.5f > viewLeft - cellWidth &&
                   cellX - cellWidth * 0.5f < viewRight + cellWidth;
        }
    }
    #endregion

    #region Cell 管理
    private void ShowCell(int index)
    {
        if (cells[index].instance != null) return;

        GameObject cell = GetFromPool();
        RectTransform rect = cell.GetComponent<RectTransform>();
        rect.anchoredPosition = cells[index].position;
        cell.name = index.ToString();
        cell.SetActive(true);

        cells[index].instance = cell;
        onCellUpdate?.Invoke(cell, index + 1);
    }

    private void HideCell(int index)
    {
        if (cells[index].instance == null) return;

        ReturnToPool(cells[index].instance);
        cells[index].instance = null;
    }

    private void RecycleAllCells()
    {
        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i]?.instance != null)
            {
                ReturnToPool(cells[i].instance);
            }
        }
    }
    #endregion

    #region 對象池
    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }

        GameObject newCell = Instantiate(cellPrefab, content);
        newCell.transform.localScale = Vector3.one;
        return newCell;
    }

    private void ReturnToPool(GameObject cell)
    {
        if (cell == null) return;
        cell.SetActive(false);
        pool.Push(cell);
    }
    #endregion

    #region 滾動事件
    private void OnScroll(Vector2 _)
    {
        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            bool inView = IsInViewport(cells[i].position);

            if (inView && cells[i].instance == null)
            {
                ShowCell(i);
            }
            else if (!inView && cells[i].instance != null)
            {
                HideCell(i);
            }
        }

        UpdateArrows();
    }

    private void UpdateArrows()
    {
        if (arrowStart == null && arrowEnd == null) return;

        float normalized = direction == Direction.Vertical
            ? scrollRect.verticalNormalizedPosition
            : scrollRect.horizontalNormalizedPosition;

        bool canScrollStart = normalized < 0.99f;
        bool canScrollEnd = normalized > 0.01f;

        if (arrowStart != null) arrowStart.SetActive(canScrollEnd);
        if (arrowEnd != null) arrowEnd.SetActive(canScrollStart);
    }
    #endregion

    #region 拖曳事件
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 上拉刷新檢測
        if (onPullRefresh != null && direction == Direction.Vertical)
        {
            if (content.anchoredPosition.y < -100f)
            {
                onPullRefresh.Invoke();
            }
        }
    }
    #endregion

    #region 公用方法
    /// <summary>
    /// 滾動到指定 index
    /// </summary>
    public void ScrollToIndex(int index)
    {
        if (cells == null || index < 0 || index >= cells.Length) return;

        Vector2 targetPos = cells[index].position;

        if (direction == Direction.Vertical)
        {
            float y = -targetPos.y - viewport.rect.height * 0.5f;
            y = Mathf.Clamp(y, 0, content.sizeDelta.y - viewport.rect.height);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
        }
        else
        {
            float x = -targetPos.x + viewport.rect.width * 0.5f;
            x = Mathf.Clamp(x, -(content.sizeDelta.x - viewport.rect.width), 0);
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        }

        OnScroll(Vector2.zero);
    }

    /// <summary>
    /// 取得目前數量
    /// </summary>
    public int Count => totalCount;
    #endregion

    private void OnDestroy()
    {
        onCellUpdate = null;
        onPullRefresh = null;
    }
}
