using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class MapManager : MonoBehaviour
{
    [SerializeField] Slider slider_blood;
    [SerializeField] Text slider_blood_text;
    [SerializeField] Text text_gold;
    [SerializeField] Text text_gear;
    [SerializeField] Transform trans_mapParent;
    [SerializeField] Text text_stageName;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] Button btn_edit;

    private AsyncOperationHandle<GameObject> mapHandle;
    private GameObject currentMapInstance;
    //List<MapData> mapDatas = new List<MapData>();//關卡資料
    void Start()
    {
        AudioManager.Instance.PlayBGM("Bgm_Map", true, 1.0f);
        slider_blood.value = GameDataManager.PlayerData.currentBlood / GameDataManager.PlayerData.maxBlood;
        slider_blood_text.text = $"{GameDataManager.PlayerData.currentBlood}/{GameDataManager.PlayerData.maxBlood}";
        text_gold.text = GameDataManager.Gold.ToString();
        text_gear.text = GameDataManager.Gear.ToString();

        // Addressables載入地圖prefab
        LoadMapPrefab();

        AddEvent();
        btn_changeSkill.onClick.AddListener(async () =>
        {
            AddressableManager.LoadAndInstantiateAsync("ChangeSkillPanel", transform);
        });
        btn_edit.onClick.AddListener(() =>
        {
            AddressableManager.LoadAndInstantiateAsync("EditPanel", transform);
        });
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

        // 卸載 Addressables 資源
        UnloadMapPrefab();
    }
    async void OnCompleteMap(object[] param)
    {
        Debug.Log("完成當前地圖，準備載入下一張地圖");   
        GameObject clearMapPanel = await AddressableManager.LoadAssetAsync<GameObject>("ClearMapPanel");
        GameObject _clearMap = Instantiate(clearMapPanel, transform);
        UnloadMapPrefab();    
        await Task.Delay(100);   
        GameDataManager.CurrentMap += 1;
        GameDataManager.PreparationRoomStage = "0";//起點 初始整備室
        GameDataManager.CurrentStage = "0";
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;//回滿血
        LoadMapPrefab();
        await Task.Delay(2000);
        Destroy(_clearMap);
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

    void OnGetGear(object[] param)
    {
        int gearAmount = (int)param[0];
        GameDataManager.Gear += gearAmount;
        text_gear.text = GameDataManager.Gear.ToString();
        Debug.Log($"獲得齒輪: {gearAmount}，目前齒輪總數: {GameDataManager.Gear}");
        GoNextOpenStage();
    }
    async void OnGetSkill(object[] param)
    {
        int skillID = (int)param[0];
        if (!GameDataManager.HasSkillIDs.Contains(skillID))
        {
            GameDataManager.HasSkillIDs.Add(skillID);
            await CommonUIManager.ShowHintBubble($"獲得新技能: {SkillDatabase.GetSkillConfig(skillID).skillName}");
            Debug.Log($"獲得技能ID: {skillID}");
        }
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
    private async void LoadMapPrefab()
    {
        text_stageName.text = MapConfig.GetStageName(GameDataManager.CurrentMap);
        string mapPrefabAddress = MapConfig.GetMapAddress(GameDataManager.CurrentMap);
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
