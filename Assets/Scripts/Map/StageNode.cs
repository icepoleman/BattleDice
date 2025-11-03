using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum StageState
{
    Locked,    // 鎖定
    Unlocked,  // 解鎖但未完成
    Completed  // 已完成
}
public class StageNode : MonoBehaviour
{
    [Header("關卡設定")]
    public string stageID;              // 關卡唯一ID 1-1
    public string stageType;         // 關卡類型
    public int row;//橫排編號
    public int col;//直排編號
    public string stageInfo;       //關卡資訊

    Image stageImage;

    private Button nodeButton;
    private StageState currentState;

    void Awake()
    {
        stageImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);

        //test
        //SetData("10-10", "Battle", "敵人ID:1");
    }
    public void SetData(string id, string type, string info)
    {
        stageID = id;
        stageType = type;
        stageInfo = info;
        stageImage.sprite = Resources.Load<Sprite>($"StageIcon/" + type);
        SetRowColFromID();
    }
    //轉換字串"1-1"給row col
    void SetRowColFromID()
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

    public void SetState(StageState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 設定按鈕互動性
        // nodeButton.interactable = currentState != StageState.Locked;

        // 根據狀態顯示對應視覺
        switch (currentState)
        {
            case StageState.Locked:
                stageImage.color = Color.gray;
                break;
            case StageState.Unlocked:
                stageImage.color = Color.white;
                break;
            case StageState.Completed:
                stageImage.color = Color.green;
                break;
        }
    }

    void OnNodeClick()
    {
        switch (stageType)
        {
            case "Story":
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, stageInfo);
                break;
            case "Battle":
                //string to int
                int rep = int.Parse(stageInfo);
                Debug.Log($"進入戰鬥關卡: {stageID}" + rep);
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, rep);
                break;
            case "Shop":
                Debug.Log($"進入商店關卡: {stageID}{stageInfo}");
                break;
            case "Boss":
                Debug.Log($"進入頭目關卡: {stageID}{stageInfo}");
                break;
            case "SavePoint":
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
                break;
            case "Item":
                Debug.Log($"進入道具關卡: {stageID}{stageInfo}");
                break;
        }
        if (currentState == StageState.Unlocked)
        {
            //  StageMapManager.Instance.SelectStage(this);
        }
        else if (currentState == StageState.Completed)
        {
            // 已完成的關卡可以重複挑戰
            // StageMapManager.Instance.SelectStage(this);
        }
    }

    public void CompleteStage()
    {
        SetState(StageState.Completed);

        // 儲存進度
        //StageMapManager.Instance.SaveProgress();
    }



    public StageState GetState() => currentState;
}