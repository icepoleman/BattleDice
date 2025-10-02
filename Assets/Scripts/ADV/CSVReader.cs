using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class DialogueData
{
    public string Chapter;
    public string Character;
    public string Dialogue;
    public string Portrait;
    public string Background;
    public string[] Choices;
    public string[] JumpTo;
    public string Tag;
    public string Anim;
}

public class CSVReader
{
    //單例模式
    private static CSVReader instance;
    public static CSVReader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CSVReader();
            }
            return instance;
        }
    }
    public List<DialogueData> LoadCSV(string _fileName)
    {
        List<DialogueData> lines = new List<DialogueData>();

        string filePath = Path.Combine(Application.streamingAssetsPath, _fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ 找不到 CSV 檔案: " + filePath);
            return null;
        }

        string[] allLines = File.ReadAllLines(filePath);

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            DialogueData data = new DialogueData();
            data.Chapter = values.Length > 0 ? values[0] : "";
            data.Character = values.Length > 1 ? values[1] : "";
            data.Dialogue = values.Length > 2 ? values[2] : "";
            data.Portrait = values.Length > 3 ? values[3] : "";
            data.Background = values.Length > 4 ? values[4] : "";

            data.Choices = values.Length > 5 && !string.IsNullOrEmpty(values[5]) ? values[5].Split('|') : new string[0];
            data.JumpTo = values.Length > 6 && !string.IsNullOrEmpty(values[6]) ? values[6].Split('|') : new string[0];

            data.Tag = values.Length > 7 ? values[7] : "";
            data.Anim = values.Length > 8 ? values[8] : "";

            lines.Add(data);
        }

        Debug.Log($"✅ 從 StreamingAssets 載入完成，共 {lines.Count} 行: {filePath}");
        Debug.Log(lines[1].Dialogue); // 測試讀取第一行的對話內容
        return lines;
    }
}

