using UnityEngine;

/// <summary>
/// 資源載入器 - 管理遊戲中的資源載入
/// 
/// 使用方法：
/// // 獲取指定點數的骰子
/// Sprite dice3 = ResourcesLoader.GetDiceSprite(3);
/// 
/// // 獲取所有骰子圖片
/// Sprite[] allDice = ResourcesLoader.GetAllDiceSprites();
/// 
/// // 手動載入資源（可選）
/// ResourcesLoader.LoadDiceResources();
/// </summary>
public static class ResourcesLoader
{
    private static Sprite[] diceSprites;
    private static bool isLoaded = false;
    
    /// <summary>
    /// 載入骰子資源
    /// </summary>
    public static void LoadDiceResources()
    {
        if (!isLoaded)
        {
            diceSprites = Resources.LoadAll<Sprite>("dice/dice");
            isLoaded = true;
            Debug.Log($"骰子資源載入完成，共 {diceSprites.Length} 張圖片");
        }
    }
    
    /// <summary>
    /// 獲取指定點數的骰子圖片（點數 1-6）
    /// </summary>
    /// <param name="diceValue">骰子點數 (1-6)</param>
    /// <returns>骰子圖片</returns>
    public static Sprite GetDiceSprite(int diceValue)
    {
        // 確保資源已載入
        if (!isLoaded)
        {
            LoadDiceResources();
        }
        
        // 檢查參數範圍
        if (diceValue < 1 || diceValue > 6)
        {
            Debug.LogWarning($"無效的骰子點數: {diceValue}，應為 1-6");
            return null;
        }
        
        // 檢查資源是否正確載入
        if (diceSprites == null || diceSprites.Length == 0)
        {
            Debug.LogError("骰子資源載入失敗或為空");
            return null;
        }
        
        // 骰子點數轉換為陣列索引 (1->0, 2->1, ..., 6->5)
        int index = diceValue - 1;
        
        if (index >= diceSprites.Length)
        {
            Debug.LogWarning($"骰子圖片索引 {index} 超出範圍，總數: {diceSprites.Length}");
            return null;
        }
        
        return diceSprites[index];
    }
    
    /// <summary>
    /// 獲取所有骰子圖片
    /// </summary>
    /// <returns>骰子圖片陣列</returns>
    public static Sprite[] GetAllDiceSprites()
    {
        // 確保資源已載入
        if (!isLoaded)
        {
            LoadDiceResources();
        }
        
        return diceSprites;
    }
    
    /// <summary>
    /// 獲取骰子圖片總數
    /// </summary>
    /// <returns>骰子圖片數量</returns>
    public static int GetDiceCount()
    {
        if (!isLoaded)
        {
            LoadDiceResources();
        }
        
        return diceSprites?.Length ?? 0;
    }
    
    /// <summary>
    /// 檢查骰子資源是否已載入
    /// </summary>
    /// <returns>是否已載入</returns>
    public static bool IsLoaded()
    {
        return isLoaded && diceSprites != null && diceSprites.Length > 0;
    }
    
    /// <summary>
    /// 重新載入骰子資源
    /// </summary>
    public static void ReloadDiceResources()
    {
        isLoaded = false;
        diceSprites = null;
        LoadDiceResources();
    }
}
