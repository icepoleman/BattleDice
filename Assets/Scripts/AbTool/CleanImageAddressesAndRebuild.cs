using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
// 自動整理圖片 Address 並重建 Addressables
public class AddressablesImageAutoAssign
{
    [MenuItem("Tools/Addressables/整理圖片 Address + 重建")]
    static void CleanImageAddressesAndRebuild()
    {
        // ✅ 你要掃的根資料夾（可改）
        string rootFolder = "Assets/DiceGame_ab/RolePic";

        // ✅ 取得 Addressables 設定
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("找不到 AddressableAssetSettings！（請先建立 Addressables）");
            return;
        }

        // ✅ 遞迴取得該資料夾內所有圖片
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { rootFolder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // ✅ 避免 meta 或不合法路徑
            if (path.EndsWith(".meta")) continue;

            // ✅ 建立 Address（子資料夾/檔名）
            string relativePath = path.Replace(rootFolder + "/", "");
            string fileName = Path.GetFileNameWithoutExtension(relativePath);
            string folder = Path.GetDirectoryName(relativePath).Replace("\\", "/");

            string newAddress = $"{folder}/{fileName}";

            // ✅ 加入 Addressables
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            if (entry == null)
            {
                // 放到預設 Group（你也可以指定你的 Group 名稱）
                AddressableAssetGroup defaultGroup = settings.DefaultGroup;
                entry = settings.CreateOrMoveEntry(guid, defaultGroup);
            }

            entry.SetAddress(newAddress);

            Debug.Log($"✅ Address 設定：{newAddress}");
        }

        // ✅ 存檔
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ✅ 自動 rebuild Addressables
        AddressableAssetSettings.BuildPlayerContent();

        Debug.Log("🎉 完成：所有圖片 Address 整理完畢並重新 Build！");
    }
}
