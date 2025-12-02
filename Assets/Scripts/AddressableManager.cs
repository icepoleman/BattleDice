using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressables 資源管理器 - 統一管理所有 Addressable 資源載入和釋放
/// 
/// 使用範例：
/// // 載入單個資源
/// Sprite sprite = await AddressableManager.LoadAssetAsync<Sprite>("enemy_001");
/// 
/// // 載入 Label 群組
/// List<Sprite> sprites = await AddressableManager.LoadLabelAsync<Sprite>("Enemies");
/// 
/// // 檢查資源是否已載入
/// bool loaded = AddressableManager.IsAssetLoaded("enemy_001");
/// 
/// // 釋放單個資源
/// AddressableManager.ReleaseAsset("enemy_001");
/// 
/// // 釋放 Label 群組
/// AddressableManager.ReleaseLabel("Enemies");
/// 
/// // 釋放所有資源
/// AddressableManager.ReleaseAll();
/// </summary>
public static class AddressableManager
{
    // 統一的資源快取 - Key: address, Value: (handle, asset, refCount)
    private static Dictionary<string, (AsyncOperationHandle handle, object asset, int refCount)> assetCache 
        = new Dictionary<string, (AsyncOperationHandle, object, int)>();
    
    // Label 群組的映射 - Key: label, Value: asset addresses list
    private static Dictionary<string, (AsyncOperationHandle handle, List<string> assetAddresses)> labelCache 
        = new Dictionary<string, (AsyncOperationHandle, List<string>)>();
    
    /// <summary>
    /// 載入單個資源
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="address">資源地址</param>
    /// <returns>載入的資源</returns>
    public static async Task<T> LoadAssetAsync<T>(string address) where T : class
    {
        // 檢查快取
        if (assetCache.TryGetValue(address, out var cachedItem))
        {
            // 增加引用計數
            assetCache[address] = (cachedItem.handle, cachedItem.asset, cachedItem.refCount + 1);
            Debug.Log($"[AddressableManager] 從快取獲取資源: {address} (引用計數: {cachedItem.refCount + 1})");
            return cachedItem.asset as T;
        }
        
        Debug.Log($"[AddressableManager] 開始載入資源: {address}");
        
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            T asset = await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 存入快取，初始引用計數為 1
                assetCache[address] = (handle, asset, 1);
                Debug.Log($"[AddressableManager] 資源載入成功: {address}");
                return asset;
            }
            else
            {
                Debug.LogError($"[AddressableManager] 資源載入失敗: {address} - {handle.OperationException}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AddressableManager] 載入資源時發生異常: {address} - {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 載入 Label 群組資源
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="label">Label 名稱</param>
    /// <returns>載入的資源列表</returns>
    public static async Task<List<T>> LoadLabelAsync<T>(string label) where T : class
    {
        // 檢查 Label 快取
        if (labelCache.TryGetValue(label, out var labelCacheItem))
        {
            Debug.Log($"[AddressableManager] 從快取獲取 Label: {label}");
            var cachedAssets = new List<T>();
            foreach (var address in labelCacheItem.assetAddresses)
            {
                if (assetCache.TryGetValue(address, out var assetItem))
                {
                    if (assetItem.asset is T asset)
                    {
                        cachedAssets.Add(asset);
                    }
                }
            }
            return cachedAssets;
        }
        
        Debug.Log($"[AddressableManager] 開始載入 Label: {label}");
        
        try
        {
            var assetList = new List<T>();
            var addressList = new List<string>();
            
            var handle = Addressables.LoadAssetsAsync<T>(
                label,
                asset =>
                {
                    if (asset != null)
                    {
                        assetList.Add(asset);
                        
                        // 嘗試獲取資源的地址
                        string assetAddress = GetAssetAddress(asset);
                        if (!string.IsNullOrEmpty(assetAddress))
                        {
                            addressList.Add(assetAddress);
                            
                            // 將資源加入統一快取，如果已存在則增加引用計數
                            if (assetCache.TryGetValue(assetAddress, out var existingItem))
                            {
                                assetCache[assetAddress] = (existingItem.handle, existingItem.asset, existingItem.refCount + 1);
                            }
                            else
                            {
                                // 為 Label 載入的資源創建一個虛擬 handle（實際 handle 是整個 Label 的）
                                assetCache[assetAddress] = (new AsyncOperationHandle(), asset, 1);
                            }
                        }
                        
                        Debug.Log($"[AddressableManager] 載入 Label 資源: {label}/{asset.ToString()}");
                    }
                }
            );
            
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 存入 Label 快取
                labelCache[label] = (handle, addressList);
                
                Debug.Log($"[AddressableManager] Label 載入完成: {label}，共 {assetList.Count} 個資源");
                return assetList;
            }
            else
            {
                Debug.LogError($"[AddressableManager] Label 載入失敗: {label} - {handle.OperationException}");
                return new List<T>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AddressableManager] 載入 Label 時發生異常: {label} - {e.Message}");
            return new List<T>();
        }
    }
    
    /// <summary>
    /// 預載入資源（載入但不返回）
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="address">資源地址</param>
    public static async Task PreloadAssetAsync<T>(string address) where T : class
    {
        await LoadAssetAsync<T>(address);
    }
    
    /// <summary>
    /// 預載入 Label 群組（載入但不返回）
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="label">Label 名稱</param>
    public static async Task PreloadLabelAsync<T>(string label) where T : class
    {
        await LoadLabelAsync<T>(label);
    }
    
    /// <summary>
    /// 獲取已載入的資源
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="address">資源地址</param>
    /// <returns>資源，如果未載入返回 null</returns>
    public static T GetLoadedAsset<T>(string address) where T : class
    {
        if (assetCache.TryGetValue(address, out var cachedItem))
        {
            return cachedItem.asset as T;
        }
        return null;
    }
    
    /// <summary>
    /// 獲取已載入的 Label 資源
    /// </summary>
    /// <typeparam name="T">資源類型</typeparam>
    /// <param name="label">Label 名稱</param>
    /// <returns>資源列表，如果未載入返回空列表</returns>
    public static List<T> GetLoadedLabel<T>(string label) where T : class
    {
        if (labelCache.TryGetValue(label, out var labelItem))
        {
            var assets = new List<T>();
            foreach (var address in labelItem.assetAddresses)
            {
                if (assetCache.TryGetValue(address, out var assetItem))
                {
                    if (assetItem.asset is T asset)
                    {
                        assets.Add(asset);
                    }
                }
            }
            return assets;
        }
        return new List<T>();
    }
    
    /// <summary>
    /// 檢查資源是否已載入
    /// </summary>
    /// <param name="address">資源地址</param>
    /// <returns>是否已載入</returns>
    public static bool IsAssetLoaded(string address)
    {
        return assetCache.ContainsKey(address);
    }
    
    /// <summary>
    /// 檢查 Label 是否已載入
    /// </summary>
    /// <param name="label">Label 名稱</param>
    /// <returns>是否已載入</returns>
    public static bool IsLabelLoaded(string label)
    {
        return labelCache.ContainsKey(label);
    }
    
    /// <summary>
    /// 釋放單個資源
    /// </summary>
    /// <param name="address">資源地址</param>
    public static void ReleaseAsset(string address)
    {
        if (assetCache.TryGetValue(address, out var cachedItem))
        {
            var newRefCount = cachedItem.refCount - 1;
            
            if (newRefCount <= 0)
            {
                // 引用計數歸零，真正釋放資源
                if (cachedItem.handle.IsValid())
                {
                    Addressables.Release(cachedItem.handle);
                }
                assetCache.Remove(address);
                Debug.Log($"[AddressableManager] 已完全釋放資源: {address}");
            }
            else
            {
                // 還有其他引用，只減少計數
                assetCache[address] = (cachedItem.handle, cachedItem.asset, newRefCount);
                Debug.Log($"[AddressableManager] 減少引用計數: {address} (剩餘: {newRefCount})");
            }
        }
    }
    
    /// <summary>
    /// 釋放 Label 群組資源
    /// </summary>
    /// <param name="label">Label 名稱</param>
    public static void ReleaseLabel(string label)
    {
        if (labelCache.TryGetValue(label, out var labelItem))
        {
            // 減少每個資源的引用計數
            foreach (var address in labelItem.assetAddresses)
            {
                ReleaseAsset(address);
            }
            
            // 釋放整個 Label 的 handle
            if (labelItem.handle.IsValid())
            {
                Addressables.Release(labelItem.handle);
            }
            
            labelCache.Remove(label);
            Debug.Log($"[AddressableManager] 已釋放 Label: {label}");
        }
    }
    
    /// <summary>
    /// 釋放所有資源
    /// </summary>
    public static void ReleaseAll()
    {
        Debug.Log("[AddressableManager] 開始釋放所有資源");
        
        // 釋放所有 Label
        foreach (var kv in labelCache)
        {
            if (kv.Value.handle.IsValid())
            {
                Addressables.Release(kv.Value.handle);
            }
        }
        
        // 釋放所有單個資源
        foreach (var kv in assetCache)
        {
            if (kv.Value.handle.IsValid())
            {
                Addressables.Release(kv.Value.handle);
            }
        }
        
        // 清空快取
        assetCache.Clear();
        labelCache.Clear();
        
        Debug.Log("[AddressableManager] 所有資源已釋放");
    }
    
    /// <summary>
    /// 獲取已載入資源的統計信息
    /// </summary>
    public static void LogLoadedAssets()
    {
        Debug.Log($"[AddressableManager] 已載入單個資源: {assetCache.Count} 個");
        Debug.Log($"[AddressableManager] 已載入 Label 群組: {labelCache.Count} 個");
        
        foreach (var address in assetCache.Keys)
        {
            Debug.Log($"  - 單個資源: {address}");
        }
        
        foreach (var label in labelCache.Keys)
        {
            Debug.Log($"  - Label 群組: {label}");
        }
    }
    
    // 輔助方法：獲取資源的地址
    private static string GetAssetAddress(object asset)
    {
        // 這裡需要根據實際情況實作
        // 可能需要使用反射或其他方式獲取資源地址
        // 暫時使用資源名稱作為地址
        return asset?.ToString() ?? string.Empty;
    }
    
    // 輔助方法：轉換 IList<object> 到 List<T>
    private static List<T> ConvertToList<T>(IList<object> objectList) where T : class
    {
        var result = new List<T>();
        foreach (var obj in objectList)
        {
            if (obj is T typedObj)
            {
                result.Add(typedObj);
            }
        }
        return result;
    }
}