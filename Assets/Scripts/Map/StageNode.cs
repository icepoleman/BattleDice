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
    //"關卡設定"
    string stageID;              // 關卡唯一ID 1-1
    string stageType;         // 關卡類型
    string stageInfo;       //關卡資訊
    int myRow;                    //關卡橫排

    Image stageImage;
    Button nodeButton;
    StageState currentState;

    void Awake()
    {
        stageImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);

        EventCenter.AddListener(MapEvent.EVENT_OPEN_STAGE_NODE, OnOpenStageNode);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_ROW_STAGE_NODE, OnOpenRowStageNode);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_STAGE_NODE, OnOpenStageNode);
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_ROW_STAGE_NODE, OnOpenRowStageNode);
    }
    public void SetData(string id, string type, string info, int row)
    {
        stageID = id;
        stageType = type;
        stageInfo = info;
        stageImage.sprite = Resources.Load<Sprite>($"StageIcon/" + type);
        myRow = row;
        SetState(StageState.Locked);
    }
    void OnOpenStageNode(object[] param)
    {
        List<string> targetStageID = (List<string>)param[0];
        if (targetStageID.Contains(stageID))
        {
            SetState(StageState.Unlocked);
        }
    }
    void OnOpenRowStageNode(object[] param)
    {
        int targetRow = (int)param[0];
        if (myRow == targetRow)
        {
            SetState(StageState.Unlocked);
        }
    }
    void SetState(StageState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 設定按鈕互動性
        nodeButton.interactable = currentState == StageState.Unlocked;

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
        if (currentState == StageState.Unlocked)
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
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_SHOP);
                    Debug.Log($"進入商店關卡: {stageID}{stageInfo}");
                    break;
                case "Boss":
                    Debug.Log($"進入頭目關卡: {stageID}{stageInfo}");
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, int.Parse(stageInfo));
                    break;
                case "SavePoint":
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
                    break;
                case "Item":
                    Debug.Log($"進入道具關卡: {stageID}{stageInfo}");
                    break;
                case "Blood":
                    Debug.Log($"回復血量");
                    break;
                case "Gold":
                    Debug.Log($"金幣: {stageInfo}");
                    break;
                case "Skill":
                    Debug.Log($"技能: {stageInfo}");
                    break;
            }
            GameDataManager.CurrentStage = stageID;
        }
        else if (currentState == StageState.Completed)
        {
            // 已完成的關卡可以重複挑戰

        }
    }

    void CompleteStage()
    {
        SetState(StageState.Completed);

        // 儲存進度
        //StageMapManager.Instance.SaveProgress();
    }
}