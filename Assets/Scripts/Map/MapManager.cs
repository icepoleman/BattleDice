using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using TMPro;

public class MapManager : MonoBehaviour
{
    [SerializeField] Image img_bloodFill;
    [SerializeField] TextMeshProUGUI txt_blood;
    [SerializeField] TextMeshProUGUI text_gold;
    [SerializeField] TextMeshProUGUI text_gear;
    [SerializeField] Transform trans_mapParent;
    [SerializeField] TextMeshProUGUI text_stageName;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] Button btn_edit;
    [SerializeField] TreasureBoxGame treasureBox;

    private Scrollbar scrollbar_map;

    private AsyncOperationHandle<GameObject> mapHandle;
    private GameObject currentMapInstance;
    //List<MapData> mapDatas = new List<MapData>();//關卡資料
    async void Start()
    {
        AudioManager.Instance.PlayBGM("Bgm_Map", true, 1.0f);
        UpdateBloodFill();
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
        EventCenter.AddListener(MapEvent.EVENT_GET_GOLD, OnGetGold);
        EventCenter.AddListener(MapEvent.EVENT_SPEND_GOLD, OnSpendGold);
        EventCenter.AddListener(MapEvent.EVENT_GET_ITEM, OnGetItem);
        EventCenter.AddListener(MapEvent.EVENT_GET_GEAR, OnGetGear);
        EventCenter.AddListener(MapEvent.EVENT_GET_SKILL, OnGetSkill);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_MAP_SHOP, OnOpenMapShop);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_STORY, OnEnterStageStory);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_BATTLE, OnEnterStageBattle);
        EventCenter.AddListener(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT, OnEnterStageSavepoint);
        EventCenter.AddListener(MapEvent.EVENT_OPEN_TREASURE_BOX, OnOpenTreasureBox);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_COMPLETE_MAP, OnCompleteMap);
        EventCenter.RemoveListener(MapEvent.EVENT_RECOVER_HEALTH, OnRecoverHealth);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_GOLD, OnGetGold);
        EventCenter.RemoveListener(MapEvent.EVENT_SPEND_GOLD, OnSpendGold);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_ITEM, OnGetItem);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_GEAR, OnGetGear);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_SKILL, OnGetSkill);
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_MAP_SHOP, OnOpenMapShop);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_STORY, OnEnterStageStory);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_BATTLE, OnEnterStageBattle);
        EventCenter.RemoveListener(MapEvent.EVENT_ENTER_STAGE_SAVEPOINT, OnEnterStageSavepoint);
        EventCenter.RemoveListener(MapEvent.EVENT_OPEN_TREASURE_BOX, OnOpenTreasureBox);
        // 卸載 Addressables 資源
        UnloadMapPrefab();
    }
    async void OnCompleteMap(object[] param)
    {
        Debug.Log("完成當前地圖，準備載入下一張地圖");
        // GameObject _clearMap = Instantiate(await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ClearMapPanel" + ".prefab"), transform);
        UnloadMapPrefab();
        await Task.Delay(100);
        Debug.LogError($"當前地圖: {GameDataManager.CurrentMap}，結束地圖: {GameDataManager.EndMap}");
        if (GameDataManager.CurrentMap == GameDataManager.EndMap)
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_END_SCENE);
        }
        else
        {
            GameDataManager.MapScrollValue = 0f;//重置地圖滾動位置
            GameDataManager.CurrentMap += 1;
            GameDataManager.PreparationRoomStage = "StartPoint";//起點 初始整備室
            GameDataManager.CurrentStage = "StartPoint";
            LoadMapPrefab();
            await Task.Delay(2000);
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
        await UIManager.ShowHintBubble(
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

    void OnOpenTreasureBox(object[] param)
    {
        TreasureBoxRewardType rewardType = (TreasureBoxRewardType)param[0];
        int rewardValue = (int)param[1];
        treasureBox.Setup(3, rewardType, rewardValue * 5);
        treasureBox.gameObject.SetActive(true);
    }
    async void OnGetGold(object[] param)
    {
        int goldAmount = (int)param[0];
        GameDataManager.Gold += goldAmount;
        text_gold.text = GameDataManager.Gold.ToString();
        await UIManager.ShowHintBubble(
         LanguageManager.GetFormat("T_GetGold", goldAmount)
        );
        Debug.Log($"獲得金幣: {goldAmount}，目前金幣總數: {GameDataManager.Gold}");
    }
    //花費金幣
    void OnSpendGold(object[] param)
    {
        int goldAmount = (int)param[0];
        GameDataManager.Gold -= goldAmount;
        if (GameDataManager.Gold < 0)
        {
            GameDataManager.Gold = 0;
        }
        text_gold.text = GameDataManager.Gold.ToString();
        Debug.Log($"花費金幣: {goldAmount}，目前金幣總數: {GameDataManager.Gold}");
    }

    async void OnGetGear(object[] param)
    {
        int gearAmount = (int)param[0];
        GameDataManager.Gear += gearAmount;
        text_gear.text = GameDataManager.Gear.ToString();
        await UIManager.ShowHintBubble(
          LanguageManager.GetFormat("T_GetGear", gearAmount)
        );
        Debug.Log($"獲得齒輪: {gearAmount}，目前齒輪總數: {GameDataManager.Gear}");
    }
    async void OnGetSkill(object[] param)
    {
        int skillID = (int)param[0];
        GameDataManager.AddSkill(skillID);
    }
    async void OnOpenMapShop(object[] param)
    {
        //開啟三選一事件
        GameObject mapShopPanel = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "MapShopPanel" + ".prefab");
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
    //特殊道具用
    void OnGetItem(object[] param)
    {
        string itemName = (string)param[0];
        // 道具處理邏輯
    }

    // 載入地圖 Prefab
    private async void LoadMapPrefab()
    {
        text_stageName.text = MapConfig.GetStageName(GameDataManager.CurrentMap);
        string mapPrefabAddress = MapConfig.GetMapAddress(GameDataManager.CurrentMap);
        try
        {
            Debug.Log($"開始載入地圖: {ABconfig.MAP_PREFABS + mapPrefabAddress + ".prefab"}");

            mapHandle = Addressables.LoadAssetAsync<GameObject>(ABconfig.MAP_PREFABS + mapPrefabAddress + ".prefab");
            GameObject mapPrefab = await mapHandle.Task;

            if (mapHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // 實例化地圖 prefab
                currentMapInstance = Instantiate(mapPrefab, trans_mapParent);
                currentMapInstance.transform.localPosition = Vector3.zero;
                currentMapInstance.transform.localScale = Vector3.one;
                scrollbar_map = currentMapInstance.GetComponentInChildren<Scrollbar>();
                scrollbar_map.value = GameDataManager.MapScrollValue;
                await Task.Delay(500);
                EventCenter.Dispatch(MapEvent.EVENT_OPEN_TARGET_STAGE_NODE, GameDataManager.CurrentStage);
                await Task.Delay(1000);
                SceneLoader.HideLoadingScreen();
            }
            else
            {
                Debug.LogError($"載入地圖失敗: {mapHandle.OperationException}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"載入地圖時發生錯誤: {e.Message}");
        }
    }

    // 卸載地圖 Prefab
    private void UnloadMapPrefab()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            currentMapInstance = null;
            Debug.Log("地圖實例已銷毀");
        }

        if (mapHandle.IsValid())
        {
            Addressables.Release(mapHandle);
            Debug.Log("地圖 Addressables 資源已釋放");
        }
    }
}
