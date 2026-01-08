using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapManager : MonoBehaviour
{
    [SerializeField] Slider slider_blood;
    [SerializeField] Text slider_blood_text;
    [SerializeField] Text text_gold;
    [SerializeField] Transform trans_mapParent;
    [SerializeField] Button btn_backToMenu;

    private AsyncOperationHandle<GameObject> mapHandle;
    private GameObject currentMapInstance;
    //List<MapData> mapDatas = new List<MapData>();//關卡資料
    void Start()
    {
        slider_blood.value = GameDataManager.PlayerData.currentBlood / GameDataManager.PlayerData.maxBlood;
        slider_blood_text.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";
        text_gold.text = GameDataManager.Gold.ToString();
        btn_backToMenu.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });

        // Addressables載入地圖prefab
        LoadMapPrefab("Map" + GameDataManager.CurrentMap);

        AddEvent();
    }
    void AddEvent()
    {
        EventCenter.AddListener(MapEvent.EVENT_RECOVER_HEALTH, OnRecoverHealth);
        EventCenter.AddListener(MapEvent.EVENT_GET_GOLD, OnGetGold);
        EventCenter.AddListener(MapEvent.EVENT_GET_ITEM, OnGetItem);
        EventCenter.AddListener(MapEvent.EVENT_GET_GEAR, OnGetGear);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(MapEvent.EVENT_RECOVER_HEALTH, OnRecoverHealth);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_GOLD, OnGetGold);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_ITEM, OnGetItem);
        EventCenter.RemoveListener(MapEvent.EVENT_GET_GEAR, OnGetGear);

        // 卸載 Addressables 資源
        UnloadMapPrefab();
    }
    void OnRecoverHealth(object[] param)
    {
        int recoverAmount = (int)param[0];
        GameDataManager.PlayerData.currentBlood += recoverAmount;
        if (GameDataManager.PlayerData.currentBlood > GameDataManager.PlayerData.maxBlood)
        {
            GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        }
        slider_blood.value = GameDataManager.PlayerData.currentBlood / GameDataManager.PlayerData.maxBlood;
        slider_blood_text.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";
        GoNextOpenStage();
    }
    void OnGetGold(object[] param)
    {
        int goldAmount = (int)param[0];
        GameDataManager.Gold += goldAmount;
        text_gold.text = GameDataManager.Gold.ToString();
        Debug.Log($"獲得金幣: {goldAmount}，目前金幣總數: {GameDataManager.Gold}");
        GoNextOpenStage();
    }
    void OnGetGear(object[] param)
    {
        int gearAmount = (int)param[0];
        GameDataManager.GearNum += gearAmount;
        Debug.Log($"獲得齒輪: {gearAmount}，目前齒輪總數: {GameDataManager.GearNum}");
        GoNextOpenStage();
    }
    //特殊道具用
    void OnGetItem(object[] param)
    {
        string itemName = (string)param[0];
        GoNextOpenStage();
    }
    void GoNextOpenStage()
    {
        EventCenter.Dispatch(MapEvent.EVENT_OPEN_NEXT_STAGE_NODE, GameDataManager.CurrentStage);
    }

    // 載入地圖 Prefab
    private async void LoadMapPrefab(string mapPrefabAddress)
    {
        try
        {
            Debug.Log($"開始載入地圖: {mapPrefabAddress}");

            mapHandle = Addressables.LoadAssetAsync<GameObject>(mapPrefabAddress);
            GameObject mapPrefab = await mapHandle.Task;

            if (mapHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // 實例化地圖 prefab
                currentMapInstance = Instantiate(mapPrefab, trans_mapParent);
                currentMapInstance.transform.localPosition = Vector3.zero;
                currentMapInstance.transform.localScale = Vector3.one;
                GoNextOpenStage();
                Debug.Log("地圖載入並實例化成功");
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
