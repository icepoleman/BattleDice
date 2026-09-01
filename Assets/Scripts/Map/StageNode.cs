using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    Battle_Hard,//困難戰鬥關卡
    Battle_Boss,
    SavePoint,
    Blood,
    MapShop,//地圖商店
    Gold,
    Skill,
    Gear,
    Takara,//寶箱
}
public class StageNode : MonoBehaviour
{
    [Header("關卡類型")]
    [SerializeField] StageType stageType;
    [Header("關卡資訊")]
    [SerializeField] string stageInfo;       //關卡資訊
    [Header("完成後劇情(可選)")]
    [SerializeField] string completedStory;       //完成後劇情(可選) 用完清空

    [Header("連線設定")]
    [SerializeField] private Transform lineRoot; // 用於存放連線的父物件
    [SerializeField] private GameObject linePrefab; // 連線的Prefab
    [Header("節點圖片")]
    [SerializeField] Sprite[] stageSpList;
    Image stageImage;
    Button nodeButton;
    public StageState currentState;
    [SerializeField] List<StageNode> nextStageNodes = new List<StageNode>();
    //[Header("滑鼠效果")]
    private BaseMouseEffect mouseEffect; // 用於滑鼠效果的組件
    private OnMouseOutline mouseOutline; // 用於點擊效果的組件

    string stageID => gameObject.name; // 使用 GameObject 的名稱作為關卡 ID
    void Awake()
    {
        EventCenter.AddListener(MapEvent.EVENT_OPEN_TARGET_STAGE_NODE, OnOpenTargetStageNode);
        EventCenter.AddListener(MapEvent.EVENT_HIDE_ALL_STAGE, OnHideStage);
    }
    public void SetStageInfo(string _info)//測試用
    {
        stageInfo = _info;
    }
    async void GoNextStage()
    {
        EventCenter.Dispatch(MapEvent.EVENT_HIDE_ALL_STAGE);
        await Task.Delay(100); // 等待動畫或過場效果
        if (nextStageNodes.Count > 0)
        {
            foreach (var node in nextStageNodes)
            {
                node.SetState(StageState.Unlocked);
            }
        }
        else
        {
            EventCenter.Dispatch(MapEvent.EVENT_COMPLETE_MAP);
        }
    }
    async void Start()
    {
        mouseEffect = GetComponent<BaseMouseEffect>();
        mouseOutline = GetComponent<OnMouseOutline>();
        stageImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);

        //lineSprite = Resources.Load<Sprite>("Sprites/LineSprite"); // 確保有一個名為 LineSprite 的白色方形圖片在 Resources/Sprites 資料夾中

        if (stageSpList == null || stageSpList.Length == 0)
        {
            await Task.Yield();
            Canvas.ForceUpdateCanvases();
            DrawLinesToNextNodes();
            return;
        }
        switch (stageType)
        {
            case StageType.Gold:
            case StageType.Gear:
            case StageType.Skill:
            case StageType.Takara:
                stageImage.sprite = stageSpList[7];
                break;
            case StageType.Battle_Boss:
                //stageImage.sprite = stageSpList[(int)stageType];
                break;
            default:
                stageImage.sprite = stageSpList[(int)stageType];
                break;
        }

        await Task.Yield();
        Canvas.ForceUpdateCanvases();
        DrawLinesToNextNodes();
    }

    void DrawLinesToNextNodes()
    {
        if (nextStageNodes == null || nextStageNodes.Count == 0)
            return;

        RectTransform fromRect = transform as RectTransform;
        RectTransform lineRootRect = lineRoot as RectTransform;
        if (fromRect == null || lineRootRect == null)
        {
            Debug.LogWarning($"[{name}] 節點或 lineRoot 不是 UI RectTransform，無法繪製連線");
            return;
        }

        foreach (var node in nextStageNodes)
        {
            if (node == null) continue;

            RectTransform toRect = node.transform as RectTransform;
            GameObject lineObj = Instantiate(linePrefab, lineRoot);
            lineObj.name = $"Line__To_{node.name}";

            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            if (lineRect == null || toRect == null)
            {
                Debug.LogWarning($"[{name}] 連線或節點不是 UI RectTransform，略過連線 {node.name}");
                Destroy(lineObj);
                continue;
            }

            Vector3 startWorld = fromRect.TransformPoint(fromRect.rect.center);
            Vector3 endWorld = toRect.TransformPoint(toRect.rect.center);

            Vector2 startLocal = lineRootRect.InverseTransformPoint(startWorld);
            Vector2 endLocal = lineRootRect.InverseTransformPoint(endWorld);

            Vector2 dir = endLocal - startLocal;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Vector2 lineStartOffset = new Vector2(140f, angle);
            lineRect.localScale = new Vector3(0.3f, 0.3f, 1f);
            lineRect.anchoredPosition = startLocal + lineStartOffset;
            lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);

            Vector2 size = lineRect.sizeDelta;
            size.x = distance / lineRect.localScale.x;
            lineRect.sizeDelta = size * 0.5f;
        }
    }

    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_TARGET_STAGE_NODE, OnOpenTargetStageNode);
        EventCenter.RemoveListener(MapEvent.EVENT_HIDE_ALL_STAGE, OnHideStage);
    }
    void OnHideStage(object[] param)
    {
        if (!GameDataManager.TestMode)
            SetState(StageState.Locked);
    }
    public async void OnOpenTargetStageNode(object[] param)
    {
        string targetStage = (string)param[0];
        if (targetStage == gameObject.name)
        {
            GoNextStage();
            await Task.Delay(500);
            SetState(StageState.Completed);
        }
    }
    public void SetState(StageState newState)
    {
        currentState = newState;

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
        if (stageType == StageType.SavePoint && currentState == StageState.Completed)//整備室完成後仍然可以點擊
        {
            nodeButton.interactable = true;
            stageImage.color = Color.white;
            mouseEffect.EffectEnabled = true;
            mouseOutline.EffectEnabled = true;
        }
        else
        {
            bool isOpen = currentState == StageState.Unlocked;
            nodeButton.interactable = isOpen;
            mouseEffect.EffectEnabled = isOpen;
            mouseOutline.EffectEnabled = isOpen;
        }
    }

    void OnNodeClick()
    {
        if (currentState != StageState.Locked ||GameDataManager.TestMode)
        {
            GameDataManager.TmpCompletedStory = completedStory;
            GameDataManager.CurrentStage = stageID;
            switch (stageType)
            {
                case StageType.Story:
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_STORY, stageInfo);
                    break;
                case StageType.Battle_Boss:
                case StageType.Battle_Hard:
                case StageType.Battle:
                    //string to int
                    Debug.Log($"進入戰鬥關卡: {stageID}{stageInfo}");
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_BATTLE, int.Parse(stageInfo));
                    break;
                case StageType.SavePoint:
                    GameDataManager.SocialPoint += 1;//每次進入整備室增加社交點數
                    if (stageInfo != "")
                    {
                        GameDataManager.SafeRoomLevel = int.Parse(stageInfo);
                        Debug.Log($"進入整備室關卡: {stageID}，整備室等級提升至 {stageInfo}");
                    }
                    GameDataManager.PreparationRoomStage = stageID; //記錄當前整備室關卡
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT);
                    break;
                /* case StageType.Item:
                     Debug.Log($"取得道具: {stageID}{stageInfo}");
                     EventCenter.Dispatch(StateEvent.EVENT_GET_ITEM, stageInfo); //取得道具
                     GoNextStage();
                     break;*/
                case StageType.Gear:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GEAR, int.Parse(stageInfo)); //取得齒輪
                    EventCenter.Dispatch(MapEvent.EVENT_OPEN_TREASURE_BOX); //開啟寶箱事件
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
                    EventCenter.Dispatch(StateEvent.EVENT_GET_GOLD, int.Parse(stageInfo)); //取得金幣
                                                                                           //  EventCenter.Dispatch(MapEvent.EVENT_OPEN_TREASURE_BOX);
                    Debug.Log($"金幣: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.Skill:
                    EventCenter.Dispatch(StateEvent.EVENT_GET_SKILL, int.Parse(stageInfo)); //取得技能
                    Debug.Log($"技能: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.MapShop:
                    EventCenter.Dispatch(MapEvent.EVENT_OPEN_MAP_SHOP); //開啟地圖商店
                    Debug.Log($"地圖商店: {stageInfo}");
                    GoNextStage();
                    break;
                case StageType.Takara:
                    EventCenter.Dispatch(MapEvent.EVENT_OPEN_TREASURE_BOX); //開啟寶箱
                    GoNextStage();
                    break;
            }
        }
        /* if (stageType != StageType.Battle && GameDataManager.TmpCompletedStory != "")
         {
             string tmpStory = GameDataManager.TmpCompletedStory;
             GameDataManager.TmpCompletedStory = "";
             EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_STORY, tmpStory);
         }*/
    }
}