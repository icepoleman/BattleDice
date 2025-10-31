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
    public Button btn_test;
    public int currentRow = 1;//橫排編號
    public int currentCol = 1;//直排編號
    public GameObject obj_rowParent; //行父物件
    public GameObject obj_stageNode; //關卡節點預製物件
    public Transform stageBurnPos;
    Transform nowRowTrans;
    public List<StageNode> stageNodes = new List<StageNode>();
    public List<MapData> mapDatas = new List<MapData>();
    void Start()
    {
        btn_test.onClick.AddListener(() =>
        {
            GetNextLevels(currentRow, currentCol);
        });
        //測試用
        GetNextLevels(1, 1);

        obj_rowParent.SetActive(false);
        obj_stageNode.SetActive(false);

        mapDatas = CSVReader.Instance.LoadMapCSV("map1");
        int burnRow = 0;
        //mapDatas 資料要完全相反
        mapDatas.Reverse();
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
            }
            GameObject stageNodeObj = Instantiate(obj_stageNode, nowRowTrans);
            stageNodeObj.name = "StageNode_" + mapData.stageID;
            stageNodeObj.SetActive(true);
            StageNode stageNode = stageNodeObj.GetComponent<StageNode>();
            stageNode.SetData(mapData.stageID, mapData.type, mapData.stageInfo);
            stageNodes.Add(stageNode);
            Debug.Log($"MapData: {mapData.stageID}, {mapData.type}, {mapData.stageInfo}");
        }
    }
    public void SetRowColFromID(string stageID, ref int row, ref int col)
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
    // Update is called once per frame
    void Update()
    {

    }
    int[] rowCount = { 1, 1, 5, 5, 3, 1 }; // row=1~5

    public void GetNextLevels(int row, int col)
    {
        int _row = currentRow - 1;
        int nextRow = _row + 1;

        if (nextRow >= rowCount.Length)
        {
            Debug.Log("No Next Level");
            return; // 沒下一排了
        }


        if (rowCount[_row] == 1 || rowCount[nextRow] == 1)
        {
            Debug.Log($"Next Level: Row {row + 1}, 全開");
        }

        if (rowCount[_row] == 3 && rowCount[nextRow] == 5)
        {
            Debug.Log($"Next Level: Row {row + 1}, col {col}, col+1 {col + 1}, col+2 {col + 2}開");
        }

        if (rowCount[_row] == 5 && rowCount[nextRow] == 5)
        {
            if (col == 1)
                Debug.Log($"Next Level: Row {row + 1}, col {col}, col+1 {col + 1}開");
            else if (col == 5)
                Debug.Log($"Next Level: Row {row + 1}, col {col}, col {col - 1}開");
            else
                Debug.Log($"Next Level: Row {row + 1}, col {col - 1},col {col}, col{col + 1}開");
        }
        if (rowCount[_row] == 5 && rowCount[nextRow] == 3)
        {
            if (col == 1)
                Debug.Log($"Next Level: Row {row + 1}, col {col}");
            else if (col == 2)
                Debug.Log($"Next Level: Row {row + 1}, col {col - 1}, col {col}開");
            else if (col == 3)
                Debug.Log($"Next Level: Row {row + 1}, col {col - 2},col {col - 1}, col{col}開");
            else if (col == 4)
                Debug.Log($"Next Level: Row {row + 1}, col {col - 2},col {col - 1}");
            else if (col == 5)
                Debug.Log($"Next Level: Row {row + 1}, col {col - 2}");
        }
    }
}
