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


    /// <summary>
    /// ✅ 第一次使用角色時：預載該角色全部立繪（用 Label）
    /// </summary>
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
    /// ✅ 全部清空（你如果切章節、切場景可以用）
    /// </summary>
    public static void UnloadAll()
    {
        Debug.Log("[PortraitManager] 卸載所有角色立繪");

        foreach (var kv in roleHandles)
        {
            if (kv.Value.IsValid())
            {
                Addressables.Release(kv.Value);
            }
        }

        roleHandles.Clear();
        allRoleSprites.Clear();
    }
}
