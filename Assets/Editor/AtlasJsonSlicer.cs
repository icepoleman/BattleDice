
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class AtlasJsonSlicer
{
    [MenuItem("Tools/Atlas/Import JSON Names")]
    private static void ImportNames()
    {
        var texture = Selection.activeObject as Texture2D;

        if (texture == null)
        {
            Debug.LogError("請先選取圖集 PNG。");
            return;
        }

        string texturePath = AssetDatabase.GetAssetPath(texture);
        string jsonPath = EditorUtility.OpenFilePanel(
            "選擇圖集 JSON", Application.dataPath, "json");

        if (string.IsNullOrEmpty(jsonPath))
            return;

        JObject root;
        try
        {
            root = JObject.Parse(File.ReadAllText(jsonPath));
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 讀取失敗：{e.Message}");
            return;
        }

        var frames = root["frames"];
        if (frames == null)
        {
            Debug.LogError("JSON 找不到 frames。");
            return;
        }

        var items = frames is JObject obj
            ? obj.Properties().Select(p => (Name: p.Name, Data: p.Value))
            : frames is JArray arr
                ? arr.Select(f => (
                    Name: f["filename"]?.ToString() ?? "",
                    Data: f))
                : Enumerable.Empty<(string Name, JToken Data)>();

        var importer =
            AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();

        var factory = new SpriteDataProviderFactories();
        factory.Init();

        var provider =
            factory.GetSpriteEditorDataProviderFromObject(importer);

        provider.InitSpriteEditorDataProvider();

        var spriteRects = items.Select(item =>
        {
            var frame = item.Data["frame"];
            if (frame == null)
                throw new Exception($"缺少 frame：{item.Name}");

            float x = (float)frame["x"];
            float y = (float)frame["y"];
            float w = (float)frame["w"];
            float h = (float)frame["h"];

            return new SpriteRect
            {
                name = Path.GetFileNameWithoutExtension(item.Name),
                rect = new Rect(
                    x, texture.height - y - h, w, h),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = GUID.Generate()
            };
        }).ToArray();

        if (spriteRects.Length == 0)
        {
            Debug.LogError("沒有讀取到任何圖示。");
            return;
        }

        provider.SetSpriteRects(spriteRects);
        provider.Apply();

        importer.SaveAndReimport();
        AssetDatabase.Refresh();

        Debug.Log($"完成！已切割並命名 {spriteRects.Length} 個 Sprite。");
    }
}
