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
public enum StageType
{
    Story,
    Battle,
    SavePoint,
    Item,
    Blood,
    Gold,
    Skill,
    Gear
}
public class StageNode : MonoBehaviour
{
    //"關卡設定"
    string stageID;              // 關卡唯一ID 
    [Header("關卡類型")]
    [SerializeField] StageType stageType;
    [Header("關卡資訊")]
    [SerializeField] string stageInfo;       //關卡資訊
    [Header("打贏後劇情(可選)")]
    [SerializeField] string completedStory;       //完成後劇情(可選) 用完清空

    Image stageImage;
    Button nodeButton;
    StageState currentState;
    [SerializeField] List<StageNode> nextStageNodes = new List<StageNode>();
    
    // 防連點
    private bool isProcessing = false;
    void Awake()
    {
        stageID = gameObject.name;
        stageImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);
        SetState(StageState.Locked);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_NEXT_STAGE_NODE, OnOpenNextStageNode);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_NEXT_STAGE_NODE, OnOpenNextStageNode);
    }
    public void OnOpenNextStageNode(object[] param)
    {
        string targetStage = (string)param[0];
        if (currentState == StageState.Completed)
            SetState(StageState.Locked);
        if (targetStage != stageID) return;
        Debug.Log("OnOpenNextStageNode called");
        SetState(StageState.Completed);
        if (nextStageNodes.Count > 0)
        {
            foreach (var node in nextStageNodes)
            {
                node.SetState(StageState.Unlocked);
            }
        }
        else
        {
            Debug.Log("已完成所有關卡!");
            EventCenter.Dispatch(MapEvent.EVENT_COMPLETE_MAP);//第一個節點都要是start 並快速存檔
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
        nodeButton.interactable = currentState == StageState.Unlocked;
        if (stageType == StageType.SavePoint && currentState != StageState.Locked)
        {
            nodeButton.interactable = true; //準備室關卡永遠可點
        }

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
        // 防連點
        if (isProcessing) return;
        
        if (currentState != StageState.Locked)
        {
            isProcessing = true;
            
            GameDataManager.CompletedStory = completedStory;
            GameDataManager.CurrentStage = stageID;
            switch (stageType)
            {
                case StageType.Story:
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, stageInfo);
                    break;
                case StageType.Battle:
                    //string to int
                    Debug.Log($"進入戰鬥關卡: {stageID}{stageInfo}");
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, int.Parse(stageInfo));
                    break;
                case StageType.SavePoint:
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
                    GameDataManager.PreparationRoomStage = stageID; //記錄當前整備室關卡
                    break;
                case StageType.Item:
                    Debug.Log($"取得道具: {stageID}{stageInfo}");
                    EventCenter.Dispatch(MapEvent.EVENT_GET_ITEM, stageInfo); //取得道具
                    break;
                case StageType.Gear:
                    EventCenter.Dispatch(MapEvent.EVENT_GET_GEAR, int.Parse(stageInfo)); //取得齒輪
                    Debug.Log($"齒輪: {stageInfo}");
                    break;
                case StageType.Blood:
                    EventCenter.Dispatch(MapEvent.EVENT_RECOVER_HEALTH, int.Parse(stageInfo)); //回血
                    Debug.Log($"回復血量");
                    break;
                case StageType.Gold:
                    EventCenter.Dispatch(MapEvent.EVENT_GET_GOLD, int.Parse(stageInfo)); //取得金幣
                    Debug.Log($"金幣: {stageInfo}");
                    break;
                case StageType.Skill:
                    Debug.Log($"技能: {stageInfo}");
                    break;
            }
            
            // 自動存檔
            SaveManager.AutoSave();
        }
        if (stageType != StageType.Battle && GameDataManager.CompletedStory != "")
        {
            string tmpStory = GameDataManager.CompletedStory;
            GameDataManager.CompletedStory = "";
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, tmpStory);
        }
    }
}