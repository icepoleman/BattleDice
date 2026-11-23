using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using UniRx;

public class MapManager : MonoBehaviour
{
    [SerializeField] Image img_bloodFill;
    [SerializeField] TextMeshProUGUI txt_blood;
    [SerializeField] TextMeshProUGUI text_gold;
    [SerializeField] TextMeshProUGUI text_gear;
    [SerializeField] Transform trans_mapRoot;
    [SerializeField] GameObject mapColumnPrefab;
    [SerializeField] StageNode stageNodePrefab;
    [SerializeField] TextMeshProUGUI text_stageName;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] Button btn_edit;

    [SerializeField] Scrollbar scrollbar_map;

    //List<MapData> mapDatas = new List<MapData>();//關卡資料
    async void Start()
    {
        AudioManager.Instance.PlayBGM("Bgm_Map", true, 1.0f);
        UpdateBloodFill();
        if (GameDataManager.PlayerData.currentBlood <= 0) GameDataManager.PlayerData.currentBlood = 1;
        txt_blood.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";
        text_gold.text = GameDataManager.Gold.ToString();
        text_gear.text = GameDataManager.Gear.ToString();

        // Addressables載入地圖prefab
        LoadMapPrefab();

        AddEvent();
        btn_changeSkill.onClick.AddListener(async () =>
        {
            await UIManager.ShowPanel(ABconfig.GAME_PREFABS + "ChangeSkillPanel");
        });
        btn_edit.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("EditPanel");
        });
        //AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ClearMapPanel" + ".prefab");
    }
    void AddEvent()
    {
        EventCenter.AddListener(MapEvent.EVENT_COMPLETE_MAP, OnCompleteMap);
        EventCenter.AddListener(MapEvent.EVENT_RECOVER_HEALTH, OnRecoverHealth);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_MAP_SHOP, OnOpenMapShop);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_STORY, OnEnterStageStory);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_BATTLE, OnEnterStageBattle);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT, OnEnterStageSavepoint);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_COMPLETE_MAP, OnCompleteMap);
        EventCenter.RemoveListener(MapEvent.EVENT_RECOVER_HEALTH, OnRecoverHealth);
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_MAP_SHOP, OnOpenMapShop);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_STORY, OnEnterStageStory);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_BATTLE, OnEnterStageBattle);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT, OnEnterStageSavepoint);
        UnloadMapPrefab();
    }
    async void OnCompleteMap(object[] param)
    {
        Debug.Log("完成當前地圖，準備載入下一張地圖");
        // GameObject _clearMap = Instantiate(await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ClearMapPanel" + ".prefab"), transform);
        UnloadMapPrefab();
        await Task.Delay(100);
        Debug.LogError($"當前地圖: {GameDataManager.CurrentMap}，結束地圖: {GameDataManager.EndMap}");
        if (GameDataManager.AllMapInfo[GameDataManager.CurrentMap].mapID == GameDataManager.EndMap)
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_END_SCENE);
        }
        else
        {
            GameDataManager.MapScrollValue = 0f;//重置地圖滾動位置
            GameDataManager.CurrentMap += 1;
            GameDataManager.PreparationRoomStage = "1-1";//起點 初始整備室
            GameDataManager.CurrentStage = "1-1";
            LoadMapPrefab();
            await Task.Delay(2000);
            EventCenter.Dispatch(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT);
            //Destroy(_clearMap);
        }
    }
    async void OnRecoverHealth(object[] param)
    {
        int recoverAmount = (int)param[0];
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        /* GameDataManager.PlayerData.currentBlood += recoverAmount;
         if (GameDataManager.PlayerData.currentBlood > GameDataManager.PlayerData.maxBlood)
         {
             GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
         }*/
        UpdateBloodFill();
        txt_blood.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";
        EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE,
         LanguageManager.GetFormat("T_RecoverHealth_max"));
    }

    void UpdateBloodFill()
    {
        if (img_bloodFill == null)
        {
            return;
        }

        float maxBlood = GameDataManager.PlayerData.maxBlood;
        float bloodRatio = maxBlood <= 0f ? 0f : Mathf.Clamp01(GameDataManager.PlayerData.currentBlood / maxBlood);
        float targetHeight = Mathf.Lerp(220f, 800f, bloodRatio);
        RectTransform rectTransform = img_bloodFill.rectTransform;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, targetHeight);
    }
    async void OnOpenMapShop(object[] param)
    {
        GameObject mapShopPanel = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ShopPanel" + ".prefab");
        //開啟三選一事件
        // GameObject mapShopPanel = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "MapShopPanel" + ".prefab");
        Instantiate(mapShopPanel, transform);
    }
    void OnEnterStageStory(object[] param)
    {
        GameDataManager.MapScrollValue = scrollbar_map.value;//記錄當前地圖滾動位置
        string stageInfo = (string)param[0];
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, stageInfo);
    }
    void OnEnterStageBattle(object[] param)
    {
        GameDataManager.MapScrollValue = scrollbar_map.value;//記錄當前地圖滾動位置
        int enemyID = (int)param[0];
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, enemyID);
    }
    void OnEnterStageSavepoint(object[] param)
    {
        GameDataManager.MapScrollValue = scrollbar_map.value;//記錄當前地圖滾動位置
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
    }
    public void RefreshResourceDisplay()
    {
        text_gold.text = GameDataManager.Gold.ToString();
        text_gear.text = GameDataManager.Gear.ToString();
    }

    // 依地圖 CSV 生成欄位與關卡節點
    private async void LoadMapPrefab()
    {
        MapInfoData nowMapInfo = GameDataManager.GetNowMapInfo();
        text_stageName.text = nowMapInfo.mapName;
        if (trans_mapRoot == null || mapColumnPrefab == null || stageNodePrefab == null)
        {
            Debug.LogError("地圖生成失敗：請設定 trans_mapRoot、mapColumnPrefab 與 stageNodePrefab。");
            return;
        }

        UnloadMapPrefab();
        List<MapData> mapDatas = CSVReader.Instance.LoadMapCSV(nowMapInfo.mapID);
        if (mapDatas == null)
        {
            return;
        }

        Dictionary<string, Transform> columnsById = new Dictionary<string, Transform>();
        Dictionary<string, StageNode> nodesById = new Dictionary<string, StageNode>();
        foreach (MapData mapData in mapDatas)
        {
            string[] stageIdParts = mapData.stageID.Split('-');
            string columnId = stageIdParts[0];
            if (!columnsById.TryGetValue(columnId, out Transform columnTransform))
            {
                GameObject column = Instantiate(mapColumnPrefab, trans_mapRoot, false);
                column.name = $"Column_{columnId}";
                columnTransform = column.transform;
                columnsById[columnId] = columnTransform;
            }

            StageNode stageNode = Instantiate(stageNodePrefab, columnTransform, false);
            stageNode.name = mapData.stageID;
            stageNode.SetMapData(mapData);
            nodesById[mapData.stageID] = stageNode;
        }

        foreach (MapData mapData in mapDatas)
        {
            if (!nodesById.TryGetValue(mapData.stageID, out StageNode stageNode))
                continue;

            List<StageNode> nextNodes = new List<StageNode>();
            foreach (string nextStageId in mapData.nextStageNodesID)
            {
                if (nodesById.TryGetValue(nextStageId, out StageNode nextNode))
                    nextNodes.Add(nextNode);
                else
                    Debug.LogWarning($"地圖 {GameDataManager.CurrentMap} 找不到連線目標節點: {nextStageId}");
            }
            stageNode.SetNextStageNodes(nextNodes);
        }

        ScrollRect scrollRect = trans_mapRoot.GetComponentInParent<ScrollRect>();
        if (scrollRect == null)
            scrollRect = trans_mapRoot.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
            scrollRect.scrollSensitivity = 20f;
        if (scrollbar_map != null && GameDataManager.MapScrollValue > 0)
            scrollbar_map.value = Mathf.Clamp01(GameDataManager.MapScrollValue + 0.06f);

        await Task.Yield();
        EventCenter.Dispatch(MapEvent.EVENT_OPEN_TARGET_STAGE_NODE, GameDataManager.CurrentStage);
        await Task.Yield();
        SceneLoader.HideLoadingScreen();
    }

    // 清除目前生成的地圖欄位
    private void UnloadMapPrefab()
    {
        if (trans_mapRoot == null)
            return;

        for (int i = trans_mapRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(trans_mapRoot.GetChild(i).gameObject);
        }
    }
}
