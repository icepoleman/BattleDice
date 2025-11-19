using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class EnemyPortraitManager
{
    // ✅ 全部載過的怪物圖片快取
    private static Dictionary<string, Dictionary<string, Sprite>> allEnemySprites
        = new Dictionary<string, Dictionary<string, Sprite>>();

    // ✅ 每個怪物的 handle（用於手動卸載）
    private static Dictionary<string, AsyncOperationHandle> enemyHandles
        = new Dictionary<string, AsyncOperationHandle>();

    // ✅ 單張怪物圖片的 handle（用於單獨載入）
    private static Dictionary<string, AsyncOperationHandle<Sprite>> singleEnemyHandles
        = new Dictionary<string, AsyncOperationHandle<Sprite>>();

    /// <summary>
    /// ✅ 載入指定怪物的全部圖片（使用 Label 或 Group）
    /// </summary>
    public static async Task<bool> LoadEnemyIfNeeded(string enemyLabel)
    {
        // 已經載過 → 無須重複載
        if (allEnemySprites.ContainsKey(enemyLabel))
        {
            Debug.Log($"[EnemyPortraitManager] 怪物 {enemyLabel} 已載入，跳過");
            return true;
        }

        Debug.Log($"[EnemyPortraitManager] 開始載入怪物：{enemyLabel}");

        allEnemySprites[enemyLabel] = new Dictionary<string, Sprite>();

        var handle = Addressables.LoadAssetsAsync<Sprite>(
            enemyLabel,
            sprite =>
            {
                if (sprite != null)
                {
                    string key = sprite.name.ToLower();
                    allEnemySprites[enemyLabel][key] = sprite;
                    Debug.Log($"[EnemyPortraitManager] 載入怪物圖片：{enemyLabel}/{key}");
                }
            }
        );

        enemyHandles[enemyLabel] = handle;

        try
        {
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[EnemyPortraitManager] {enemyLabel} 載入完成，共 {allEnemySprites[enemyLabel].Count} 張");
                return true;
            }
            else
            {
                Debug.LogError($"[EnemyPortraitManager] {enemyLabel} 載入失敗：{handle.OperationException}");
                // 清理失敗的資料
                allEnemySprites.Remove(enemyLabel);
                enemyHandles.Remove(enemyLabel);
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EnemyPortraitManager] {enemyLabel} 載入異常：{e.Message}");
            // 清理失敗的資料
            if (allEnemySprites.ContainsKey(enemyLabel))
                allEnemySprites.Remove(enemyLabel);
            if (enemyHandles.ContainsKey(enemyLabel))
                enemyHandles.Remove(enemyLabel);
            return false;
        }
    }

    /// <summary>
    /// ✅ 載入單張怪物圖片（直接用地址載入）
    /// </summary>
    public static async Task<Sprite> LoadSingleEnemySprite(string spriteAddress)
    {
        // 檢查是否已經載入過
        if (singleEnemyHandles.ContainsKey(spriteAddress))
        {
            var existingHandle = singleEnemyHandles[spriteAddress];
            if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[EnemyPortraitManager] 單張圖片 {spriteAddress} 已載入");
                return existingHandle.Result;
            }
        }

        Debug.Log($"[EnemyPortraitManager] 載入單張怪物圖片：{spriteAddress}");

        try
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(spriteAddress);
            singleEnemyHandles[spriteAddress] = handle;
            
            Sprite result = await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[EnemyPortraitManager] 單張圖片載入成功：{spriteAddress}");
                return result;
            }
            else
            {
                Debug.LogError($"[EnemyPortraitManager] 單張圖片載入失敗：{spriteAddress} - {handle.OperationException}");
                singleEnemyHandles.Remove(spriteAddress);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EnemyPortraitManager] 單張圖片載入異常：{spriteAddress} - {e.Message}");
            if (singleEnemyHandles.ContainsKey(spriteAddress))
                singleEnemyHandles.Remove(spriteAddress);
            return null;
        }
    }

    /// <summary>
    /// ✅ 獲取怪物圖片（從已載入的快取中）
    /// </summary>
    public static Sprite GetEnemySprite(string enemyLabel, string spriteName)
    {
        string key = spriteName.ToLower();

        if (!allEnemySprites.ContainsKey(enemyLabel))
        {
            Debug.LogWarning($"[EnemyPortraitManager] 怪物 {enemyLabel} 尚未載入，請先呼叫 LoadEnemyIfNeeded()");
            return null;
        }

        if (allEnemySprites[enemyLabel].TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"[EnemyPortraitManager] 找不到怪物圖片：{enemyLabel}/{key}");
            return null;
        }
    }

    /// <summary>
    /// ✅ 根據怪物ID載入對應圖片（便利方法）
    /// </summary>
    public static async Task<Sprite> LoadEnemyById(int enemyId, string spriteName = "default")
    {
        string enemyLabel = $"Enemy_{enemyId:D3}"; // 例如：Enemy_001
        
        // 先載入該怪物的全部圖片
        bool success = await LoadEnemyIfNeeded(enemyLabel);
        
        if (success)
        {
            return GetEnemySprite(enemyLabel, spriteName);
        }
        else
        {
            // 如果載入失敗，嘗試直接載入單張圖片
            string spriteAddress = $"{enemyLabel}_{spriteName}";
            return await LoadSingleEnemySprite(spriteAddress);
        }
    }

    /// <summary>
    /// ✅ 卸載指定怪物的圖片
    /// </summary>
    public static void UnloadEnemy(string enemyLabel)
    {
        if (enemyHandles.ContainsKey(enemyLabel))
        {
            Debug.Log($"[EnemyPortraitManager] 卸載怪物：{enemyLabel}");

            if (enemyHandles[enemyLabel].IsValid())
            {
                Addressables.Release(enemyHandles[enemyLabel]);
            }

            enemyHandles.Remove(enemyLabel);
            allEnemySprites.Remove(enemyLabel);
        }
    }

    /// <summary>
    /// ✅ 卸載單張怪物圖片
    /// </summary>
    public static void UnloadSingleEnemySprite(string spriteAddress)
    {
        if (singleEnemyHandles.ContainsKey(spriteAddress))
        {
            Debug.Log($"[EnemyPortraitManager] 卸載單張圖片：{spriteAddress}");

            if (singleEnemyHandles[spriteAddress].IsValid())
            {
                Addressables.Release(singleEnemyHandles[spriteAddress]);
            }

            singleEnemyHandles.Remove(spriteAddress);
        }
    }

    /// <summary>
    /// ✅ 戰鬥結束後卸載所有怪物圖片
    /// </summary>
    public static void UnloadAllEnemies()
    {
        Debug.Log("[EnemyPortraitManager] 卸載所有怪物圖片");

        // 卸載群組載入的怪物
        foreach (var kv in enemyHandles)
        {
            if (kv.Value.IsValid())
            {
                Addressables.Release(kv.Value);
            }
        }

        // 卸載單張載入的怪物
        foreach (var kv in singleEnemyHandles)
        {
            if (kv.Value.IsValid())
            {
                Addressables.Release(kv.Value);
            }
        }

        enemyHandles.Clear();
        allEnemySprites.Clear();
        singleEnemyHandles.Clear();
    }

    /// <summary>
    /// ✅ 獲取已載入的怪物列表
    /// </summary>
    public static List<string> GetLoadedEnemies()
    {
        return new List<string>(allEnemySprites.Keys);
    }

    /// <summary>
    /// ✅ 檢查怪物是否已載入
    /// </summary>
    public static bool IsEnemyLoaded(string enemyLabel)
    {
        return allEnemySprites.ContainsKey(enemyLabel);
    }
}