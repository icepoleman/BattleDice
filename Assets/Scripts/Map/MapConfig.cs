// MapConfig.cs
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class MapConfig
{
    private static Dictionary<int, string> stageNames;

    public static Dictionary<int, string> StageNames
    {
        get
        {
            if (stageNames == null)
                LoadAllMapInfoCSV();

            return stageNames;
        }
    }

    public static List<MapInfoData> LoadAllMapInfoCSV()
    {
        List<MapInfoData> mapInfos = new List<MapInfoData>();
        stageNames = new Dictionary<int, string>();

        string csvPath = Path.Combine(Application.streamingAssetsPath, "Game", "Map", "AllMapInfo.csv");
        if (!File.Exists(csvPath))
        {
            Debug.LogError("找不到地圖資訊 CSV 檔案: " + csvPath);
            return mapInfos;
        }

        string csvText = File.ReadAllText(csvPath);
        string[] allLines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        for (int i = 1; i < allLines.Length; i++)
        {
            string line = allLines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');
            MapInfoData data = new MapInfoData
            {
                mapID = values.Length > 0 ? values[0].Trim() : "",
                mapName = values.Length > 1 ? values[1].Trim() : "",
                chapter = ParseInt(values.Length > 2 ? values[2] : ""),
                addSocialPoint = ParseInt(values.Length > 3 ? values[3] : ""),
                saveRoomLevel = ParseInt(values.Length > 4 ? values[4] : "")
            };

            mapInfos.Add(data);

            if (TryGetMapNumber(data.mapID, out int mapNumber) && !stageNames.ContainsKey(mapNumber))
                stageNames.Add(mapNumber, data.mapName);
        }

        Debug.Log($"從 StreamingAssets 載入地圖資訊完成，共 {mapInfos.Count} 筆: {csvPath}");
        return mapInfos;
    }

    private static int ParseInt(string value)
    {
        if (float.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            return (int)number;

        return 0;
    }

    private static bool TryGetMapNumber(string mapID, out int mapNumber)
    {
        mapNumber = 0;
        if (string.IsNullOrWhiteSpace(mapID))
            return false;

        string numberText = mapID.Trim();
        if (numberText.StartsWith("Map", System.StringComparison.OrdinalIgnoreCase))
            numberText = numberText.Substring(3);

        int separatorIndex = numberText.IndexOf('-');
        if (separatorIndex >= 0)
            numberText = numberText.Substring(0, separatorIndex);

        return int.TryParse(numberText, out mapNumber);
    }
}