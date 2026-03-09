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
    Gear,
    MapShop//地圖商店
}
public class StageNode : MonoBehaviour
{
    //"關卡設定"
    string stageID;              // 關卡唯一ID 
    [Header("關卡類型")]
    [SerializeField] StageType stageType;
    [Header("關卡資訊")]
    [SerializeField] string stageInfo;       //關卡資訊
    [Header("完成後劇情(可選)")]
    [SerializeField] string completedStory;       //完成後劇情(可選) 用完清空
    [Header("整備室限定劇情(可選)")]
    [SerializeField] string saveRoomStory;

    [Header("連線設定")]
    [SerializeField] float lineWidth = 4f;
    [SerializeField] Color lineColor = Color.white;

    Image stageImage;
    Button nodeButton;
    StageState currentState;
    [SerializeField] List<StageNode> nextStageNodes = new List<StageNode>();

    // 防連點
    private bool isProcessing = false;

    void GoNextStage()
    {
        SetState(StageState.Locked);
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

    void Awake()
    {
        stageID = gameObject.name;
        stageImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);
        GameDataManager.TmpSaveRoomStory = saveRoomStory;//記錄整備室劇情
        SetState(StageState.Locked);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_NEXT_STAGE_NODE, OnOpenNextStageNode);

        // 繪製連線到下一個節點
        DrawLinesToNextNodes();
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
                // 自動存檔
                SaveManager.AutoSave();
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

            GameDataManager.TmpCompletedStory = completedStory;
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
                    GoNextStage();
                    break;
                case StageType.Gear:
                    EventCenter.Dispatch(MapEvent.EVENT_GET_GEAR, int.Parse(stageInfo)); //取得齒輪
                    Debug.Log($"齒輪: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.Blood:
                    if (stageInfo == "")
                        stageInfo = "1000";//如果沒有設定回血數量就默認1000
                    EventCenter.Dispatch(MapEvent.EVENT_RECOVER_HEALTH, int.Parse(stageInfo)); //回血
                    Debug.Log($"回復血量");
                    GoNextStage();
                    break;
                case StageType.Gold:
                    EventCenter.Dispatch(MapEvent.EVENT_GET_GOLD, int.Parse(stageInfo)); //取得金幣
                    Debug.Log($"金幣: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.Skill:
                    EventCenter.Dispatch(MapEvent.EVENT_GET_SKILL, int.Parse(stageInfo)); //取得技能 
                    Debug.Log($"技能: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.MapShop:
                    EventCenter.Dispatch(MapEvent.EVENT_OPEN_MAP_SHOP); //開啟地圖商店
                    Debug.Log($"地圖商店: {stageInfo}");
                    GoNextStage();
                    break;
            }
        }
        if (stageType != StageType.Battle && GameDataManager.TmpCompletedStory != "")
        {
            string tmpStory = GameDataManager.TmpCompletedStory;
            GameDataManager.TmpCompletedStory = "";
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, tmpStory);
        }
    }
    /// <summary>
    /// 繪製連線到所有 nextStageNodes
    /// </summary>
    void DrawLinesToNextNodes()
    {
        foreach (var nextNode in nextStageNodes)
        {
            if (nextNode == null) continue;
            DrawLine(transform as RectTransform, nextNode.transform as RectTransform);
        }
    }

    /// <summary>
    /// 在兩個 RectTransform 之間繪製一條線
    /// </summary>
    void DrawLine(RectTransform from, RectTransform to)
    {
        // 創建線條 GameObject
        GameObject lineObj = new GameObject($"Line_{from.name}_to_{to.name}");
        lineObj.transform.SetParent(transform.parent);
        lineObj.transform.SetAsFirstSibling(); // 放到最底層

        // 添加 Image 組件
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = lineColor;

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 計算兩點的位置
        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        // 計算距離和角度
        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 設置線條位置（中點）
        lineRect.anchoredPosition = (fromPos + toPos) / 2f;

        // 設置線條大小
        lineRect.sizeDelta = new Vector2(distance, lineWidth);

        // 設置旋轉
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        // 設置錨點和軸心
        lineRect.anchorMin = from.anchorMin;
        lineRect.anchorMax = from.anchorMax;
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.localScale = Vector3.one;
    }
}