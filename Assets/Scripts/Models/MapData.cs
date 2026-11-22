/// <summary>
/// 地圖資料 - 從 CSV 載入
/// </summary>
[System.Serializable]
public class MapData
{
    public string stageID;
    public StageType type;
    public string stageInfo;
    public string completedStory;
    public string[] nextStageNodesID;
}
