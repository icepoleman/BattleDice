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
    Heal,
    MapShop,//地圖商店
    Takara,//寶箱
    H
}
public class StageNode : MonoBehaviour
{
    [Header("關卡類型")]
    [SerializeField] StageType stageType;
    [Header("關卡資訊")]
    [SerializeField] string stageInfo;       //關卡資訊
    [Header("完成後劇情(可選)")]
    [SerializeField] string completedStory;       //完成後劇情(可選) 用完清空
                                                  // [Header("完成後獎勵(可選)")]
                                                  // [SerializeField] StageType rewardType;
                                                  //[SerializeField] string rewardInfo;       //完成後獎勵(可選) 用完清空

    [Header("連線設定")]
    [SerializeField] private Transform lineRoot; // 用於存放連線的父物件
    [SerializeField] private GameObject linePrefab; // 連線的Prefab
    [Header("走過的路徑顯示")]
    [SerializeField] private GameObject obj_completedIcon; // 用於顯示已完成路徑的物件
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
    public void SetMapData(MapData data, Sprite sp_stage)
    {
        stageType = data.type;
        stageInfo = data.stageInfo;
        completedStory = data.completedStory;
        stageImage = GetComponent<Image>();
        stageImage.sprite = sp_stage;
        stageImage.SetNativeSize();
    }
    public void SetNextStageNodes(List<StageNode> nodes)
    {
        nextStageNodes = nodes;
        Canvas.ForceUpdateCanvases();
        DrawLinesToNextNodes();
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
                stageImage.color = Color.gray;
                EventCenter.Dispatch(MapEvent.EVENT_UPDATE_MAP_BADGE_POSITION, transform.position);
                break;
        }
        bool isOpen = currentState == StageState.Unlocked;
        nodeButton.interactable = isOpen;
        mouseEffect.EffectEnabled = isOpen;
        mouseOutline.EffectEnabled = isOpen;
    }

    void OnNodeClick()
    {
        if (currentState != StageState.Locked || GameDataManager.TestMode)
        {
            GameDataManager.TmpCompletedStory = completedStory;
            GameDataManager.CurrentStage = stageID;

            if (!string.IsNullOrWhiteSpace(stageInfo) && !int.TryParse(stageInfo, out _))
            {
                EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_STORY, stageInfo);
                return;
            }
            MapInfoData nowMapInfo = GameDataManager.GetNowMapInfo();
            switch (stageType)
            {
                case StageType.Story:
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_STORY, stageInfo);
                    break;
                case StageType.Battle_Boss:
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_BATTLE, int.Parse(stageInfo));
                    break;
                case StageType.Battle_Hard:
                    List<int> enemyIds_Erito = EnemyConfig.Instance.GetEnemyIDs("Erito", nowMapInfo.chapter);
                    int randomEritoId = enemyIds_Erito[UnityEngine.Random.Range(0, enemyIds_Erito.Count)];
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_BATTLE, randomEritoId);
                    break;
                case StageType.Battle:
                    List<int> enemyIds_Zako = EnemyConfig.Instance.GetEnemyIDs("Zako", nowMapInfo.chapter);
                    int randomZakoId = enemyIds_Zako[UnityEngine.Random.Range(0, enemyIds_Zako.Count)];
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_BATTLE, randomZakoId);
                    break;
                case StageType.SavePoint:
                    GameDataManager.PreparationRoomStage = stageID; //記錄當前整備室關卡
                    EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT);
                    break;
                case StageType.Heal:
                    if (stageInfo == "")
                        stageInfo = "1000";//如果沒有設定回血數量就默認1000
                    EventCenter.Dispatch(MapEvent.EVENT_RECOVER_HEALTH, int.Parse(stageInfo)); //回血
                    Debug.Log($"回復血量");
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
    }
}