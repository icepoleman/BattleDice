using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class PortraitManager
{
    // ✅ 全部載過的角色快取（不會重複載）
    public static Dictionary<string, Dictionary<string, Sprite>> allRoleSprites
        = new Dictionary<string, Dictionary<string, Sprite>>();

    // ✅ 每個角色的 handle（用於手動卸載）
    private static Dictionary<string, AsyncOperationHandle> roleHandles
        = new Dictionary<string, AsyncOperationHandle>();

    // ✅ 怪物圖片快取（每個怪物只有一張圖片）
    public static Dictionary<string, Sprite> allMonsterSprites
        = new Dictionary<string, Sprite>();

    // ✅ 每個怪物的 handle（用於手動卸載）
    private static Dictionary<string, AsyncOperationHandle> monsterHandles
        = new Dictionary<string, AsyncOperationHandle>();

    public static async Task LoadRoleIfNeeded(string roleLabel)
    {
        // 已經載過 → 無須重複載
        if (allRoleSprites.ContainsKey(roleLabel))
            return;

        Debug.Log($"[PortraitManager] 開始載入角色：{roleLabel}");

        allRoleSprites[roleLabel] = new Dictionary<string, Sprite>();

        var handle = Addressables.LoadAssetsAsync<Sprite>(
            roleLabel,
            sprite =>
            {
                if (sprite != null)
                {
                    string key = sprite.name.ToLower();
                    allRoleSprites[roleLabel][key] = sprite;
                    Debug.Log($"[PortraitManager] 載入立繪：{roleLabel}/{key}");
                }
            }
        );

        roleHandles[roleLabel] = handle;

        try
        {
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[PortraitManager] {roleLabel} 載入完成，共 {allRoleSprites[roleLabel].Count} 張");
            }
            else
            {
                Debug.LogError($"[PortraitManager] {roleLabel} 載入失敗：{handle.OperationException}");
                // 清理失敗的資料
                allRoleSprites.Remove(roleLabel);
                roleHandles.Remove(roleLabel);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PortraitManager] {roleLabel} 載入異常：{e.Message}");
            // 清理失敗的資料
            if (allRoleSprites.ContainsKey(roleLabel))
                allRoleSprites.Remove(roleLabel);
            if (roleHandles.ContainsKey(roleLabel))
                roleHandles.Remove(roleLabel);
        }
    }


    /// <summary>
    /// ✅ 顯示該角色的表情
    /// </summary>
    public static Sprite Show(string roleLabel, string expression)
    {
        if(expression == "")
            expression = "normal";
        string key = expression.ToLower();

        if (!allRoleSprites.ContainsKey(roleLabel))
        {
            Debug.LogError($"[PortraitManager] 角色 {roleLabel} 尚未載入，請先呼叫 LoadRoleIfNeeded()");
            return null;
        }

        if (allRoleSprites[roleLabel].TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"[PortraitManager] 找不到表情：{roleLabel}/{key}");
            return null;
        }
    }


    /// <summary>
    /// ✅ 載入怪物圖片（單一固定圖片，不使用 Label）
    /// </summary>
    public static async Task LoadMonster(string monsterKey, string spritePath)
    {
        // 已經載過 → 無須重複載
        if (allMonsterSprites.ContainsKey(monsterKey))
            return;

        Debug.Log($"[PortraitManager] 開始載入怪物：{monsterKey}");

        var handle = Addressables.LoadAssetAsync<Sprite>(spritePath);

        monsterHandles[monsterKey] = handle;

        try
        {
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                allMonsterSprites[monsterKey] = handle.Result;
                Debug.Log($"[PortraitManager] 怪物 {monsterKey} 載入完成");
            }
            else
            {
                Debug.LogError($"[PortraitManager] 怪物 {monsterKey} 載入失敗：{handle.OperationException}");
                allMonsterSprites.Remove(monsterKey);
                monsterHandles.Remove(monsterKey);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PortraitManager] 怪物 {monsterKey} 載入異常：{e.Message}");
            if (allMonsterSprites.ContainsKey(monsterKey))
                allMonsterSprites.Remove(monsterKey);
            if (monsterHandles.ContainsKey(monsterKey))
                monsterHandles.Remove(monsterKey);
        }
    }


    /// <summary>
    /// ✅ 取得怪物圖片
    /// </summary>
    public static Sprite GetMonster(string monsterKey)
    {
        if (allMonsterSprites.TryGetValue(monsterKey, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"[PortraitManager] 找不到怪物：{monsterKey}");
            return null;
        }
    }


    /// <summary>
    /// ✅ 你手動呼叫 → 卸載某個角色的全部圖片
    /// </summary>
    public static void UnloadRole(string roleLabel)
    {
        if (!roleHandles.ContainsKey(roleLabel))
            return;

        Debug.Log($"[PortraitManager] 卸載角色：{roleLabel}");

        if (roleHandles[roleLabel].IsValid())
        {
            Addressables.Release(roleHandles[roleLabel]);
        }

        roleHandles.Remove(roleLabel);
        allRoleSprites.Remove(roleLabel);
    }


    /// <summary>
    /// ✅ 卸載某個怪物圖片
    /// </summary>
    public static void UnloadMonster(string monsterKey)
    {
        if (!monsterHandles.ContainsKey(monsterKey))
            return;

        Debug.Log($"[PortraitManager] 卸載怪物：{monsterKey}");

        if (monsterHandles[monsterKey].IsValid())
        {
            Addressables.Release(monsterHandles[monsterKey]);
        }

        monsterHandles.Remove(monsterKey);
        allMonsterSprites.Remove(monsterKey);
    }


    /// <summary>
    /// ✅ 全部清空（你如果切章節、切場景可以用）
    /// </summary>
    public static void UnloadAll()
    {
        Debug.Log("[PortraitManager] 卸載所有角色立繪與怪物圖片");

        foreach (var kv in roleHandles)
        {
            if (kv.Value.IsValid())
            {
                Addressables.Release(kv.Value);
            }
        }

        foreach (var kv in monsterHandles)
        {
            if (kv.Value.IsValid())
            {
                Addressables.Release(kv.Value);
            }
        }

        roleHandles.Clear();
        allRoleSprites.Clear();
        monsterHandles.Clear();
        allMonsterSprites.Clear();
    }
}
