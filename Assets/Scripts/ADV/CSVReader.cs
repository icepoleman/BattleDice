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
    public string[] Choices;   // 開門,挖洞
    public string[] JumpTo;    // Door,Hole
    public string Tag;         // 標記 (例如 Door, Hole, keep1)
    public string Anim;        // 角色動畫
}

public class CSVReader : MonoBehaviour
{
    public string fileName = "dialogue.csv"; // StreamingAssets 裡的檔名
    public List<DialogueData> lines = new List<DialogueData>();

    void Start()
    {
        LoadCSV();
    }

    void LoadCSV()
    {
        lines.Clear();

        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ 找不到 CSV 檔案: " + filePath);
            return;
        }

        string[] allLines = File.ReadAllLines(filePath);

        bool isFirstLine = true;
        foreach (string line in allLines)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // 跳過表頭
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 你現在 Excel 匯出應該是 tab 分隔 (TSV)，所以用 '\t'
            string[] values = line.Split('\t');

            DialogueData data = new DialogueData();
            data.Chapter   = values.Length > 0 ? values[0] : "";
            data.Character = values.Length > 1 ? values[1] : "";
            data.Dialogue  = values.Length > 2 ? values[2] : "";
            data.Portrait  = values.Length > 3 ? values[3] : "";
            data.Background= values.Length > 4 ? values[4] : "";

            data.Choices   = values.Length > 5 && !string.IsNullOrEmpty(values[5]) ? values[5].Split(',') : new string[0];
            data.JumpTo    = values.Length > 6 && !string.IsNullOrEmpty(values[6]) ? values[6].Split(',') : new string[0];

            data.Tag       = values.Length > 7 ? values[7] : "";
            data.Anim      = values.Length > 8 ? values[8] : "";

            lines.Add(data);
        }

        Debug.Log($"✅ 從 StreamingAssets 載入完成，共 {lines.Count} 行: {filePath}");
    }
}

