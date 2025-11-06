using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StageType
{
    Story,     // 劇情關卡
    Battle,    // 戰鬥關卡
    Shop,      // 商店
    Boss,       // 頭目關卡
    SavePoint,  // 整備室
    Item       // 道具關卡
}
public class MapManager : MonoBehaviour
{
    [SerializeField] int currentRow = 1;//橫排編號
    [SerializeField] int currentCol = 1;//直排編號
    [SerializeField] GameObject obj_rowParent; //行父物件
    [SerializeField] GameObject obj_stageNode; //關卡節點預製物件
    [SerializeField] Transform stageBurnPos;
    [SerializeField] Slider slider_blood;
    [SerializeField] Text slider_blood_text;
    Transform nowRowTrans;
    List<MapData> mapDatas = new List<MapData>();//關卡資料
    List<int> list_rowCount = new List<int>(); // row=1~5
    void Start()
    {
        obj_rowParent.SetActive(false);
        obj_stageNode.SetActive(false);
        slider_blood.value = GameDataManager.PlayerData.currentBlood / GameDataManager.PlayerData.maxBlood;
        slider_blood_text.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";

        SetRowColFromID(GameDataManager.CurrentStage, ref currentRow, ref currentCol);
        mapDatas = CSVReader.Instance.LoadMapCSV("map" + GameDataManager.CurrentMap);
        int burnRow = 0;
        list_rowCount = new List<int>();
        foreach (var mapData in mapDatas)
        {
            int row = 0, col = 0;
            SetRowColFromID(mapData.stageID, ref row, ref col);
            if (burnRow != row)
            {
                burnRow = row;
                GameObject rowObj = Instantiate(obj_rowParent, stageBurnPos);
                rowObj.name = "Row_" + row.ToString();
                nowRowTrans = rowObj.transform;
                rowObj.SetActive(true);
                list_rowCount.Add(1);
            }
            else
            {
                list_rowCount[row - 1]++;
            }
            GameObject stageNodeObj = Instantiate(obj_stageNode, nowRowTrans);
            stageNodeObj.name = "StageNode_" + mapData.stageID;
            stageNodeObj.SetActive(true);
            StageNode stageNode = stageNodeObj.GetComponent<StageNode>();
            stageNode.SetData(mapData.stageID, mapData.type, mapData.stageInfo, row);     
        }
        GetNextRowOpenStage(currentRow, currentCol);
    }
    void SetRowColFromID(string stageID, ref int row, ref int col)
    {
        string[] parts = stageID.Split('-');
        if (parts.Length == 2)
        {
            if (int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
            {
                row = r;
                col = c;
            }
            else
            {
                Debug.LogError($"無法解析關卡ID: {stageID}");
            }
        }
        else
        {
            Debug.LogError($"關卡ID格式錯誤: {stageID}");
        }
    }

    void GetNextRowOpenStage(int row, int col)
    {
        int _nowRow = list_rowCount[row - 1];
        int _nextRow = list_rowCount[row];
        
        if (_nextRow >= list_rowCount.Count)
        {
            Debug.Log("No Next Level");
            return; // 沒下一排了
        }

        if (_nowRow == 1 || _nextRow == 1)
        {
            EventCenter.Dispatch(MapEvent.EVENT_OPEN_ROW_STAGE_NODE, row + 1);
            Debug.Log($"第 {row + 1} 排全開");
        }

        if (_nowRow == 3 && _nextRow == 5)
        {
            List<string> openStages = new List<string>();
            openStages.Add($"{row + 1}-" + col);
            openStages.Add($"{row + 1}-" + (col + 1));
            openStages.Add($"{row + 1}-" + (col + 2));
            EventCenter.Dispatch(MapEvent.EVENT_OPEN_STAGE_NODE, openStages);
            Debug.Log($"Next Level: Row {openStages}");
        }

        if (_nowRow == 5 && _nextRow == 5)
        {
            List<string> openStages = new List<string>();
            if (col == 1)
            {
                openStages.Add($"{row + 1}-" + col);
                openStages.Add($"{row + 1}-" + (col + 1));
            }
            else if (col == 5)
            {
                openStages.Add($"{row + 1}-" + (col - 1));
                openStages.Add($"{row + 1}-" + col);
            }
            else
            {
                openStages.Add($"{row + 1}-" + (col - 1));
                openStages.Add($"{row + 1}-" + col);
                openStages.Add($"{row + 1}-" + (col + 1));
            }
            EventCenter.Dispatch(MapEvent.EVENT_OPEN_STAGE_NODE, openStages);
            //log出openStages
            foreach (var stage in openStages)
            {
                Debug.Log(stage);
            }
        }
        if (_nowRow == 5 && _nextRow == 3)
        {
            List<string> openStages = new List<string>();

            if (col == 1)
                openStages.Add($"{row + 1}-" + col);
            else if (col == 2)
            {
                openStages.Add($"{row + 1}-" + (col - 1));
                openStages.Add($"{row + 1}-" + col);
            }
            else if (col == 3)
            {
                openStages.Add($"{row + 1}-" + (col - 2));
                openStages.Add($"{row + 1}-" + (col - 1));
                openStages.Add($"{row + 1}-" + col);
            }
            else if (col == 4)
            {
                openStages.Add($"{row + 1}-" + (col - 2));
                openStages.Add($"{row + 1}-" + (col - 1));  
            }
            else if (col == 5)
                openStages.Add($"{row + 1}-" + (col - 2));
            EventCenter.Dispatch(MapEvent.EVENT_OPEN_STAGE_NODE, openStages);
            Debug.Log($"Next Level: Row {openStages}");
        }
    }
}
