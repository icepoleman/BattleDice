using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PreparationRoomManager : MonoBehaviour
{
    [SerializeField] Button btn_back;
    [SerializeField] Button btn_powerUp;
    [SerializeField] GameObject powerUpPanelPrefab;
    [SerializeField] Button btn_shop;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] List<Button> btn_girls; // 角色模型列表，根據角色顯示對應模型
    [SerializeField] TMPro.TextMeshProUGUI txt_socialPoint; // 社交點數顯示

    [Header("出口鐵門")]
    [SerializeField] Transform img_doorIron; // 鐵門的Transform，用於控制鐵門的動畫或位置
    [SerializeField] Image img_switch; // 開關的Image，用於顯示開關狀態
    [SerializeField] Sprite[] spSwitch; // 開關打開的Sprite
    [SerializeField] EventTrigger doorEventTrigger; // 用於觸發鐵門開啟的事件

    enum DoorState
    {
        Closed,
        Opening,
        Opened
    }

    DoorState currentDoorState = DoorState.Closed;
    bool isDoorOpenedLocked;
    bool hasDispatchedBackAfterDoorOpened;
    Coroutine doorMoveCoroutine;

    const float DoorClosedY = 0f;
    const float DoorOpeningY = 110f;
    const float DoorOpenedY = 677f;

    [SerializeField] float doorMoveDuration = 0.5f;
    [SerializeField] float doorOpenedMoveDuration = 1f;

    void Start()
    {
        PreLoadAssets();
        SetUpDoorSwitchEventTrigger();
        ChangeDoorState(DoorState.Closed);

        btn_back.onClick.AddListener(OnBackButtonClick);
        btn_powerUp.onClick.AddListener(() =>
        {
            Instantiate(powerUpPanelPrefab, transform);
        });
        btn_shop.onClick.AddListener(async () =>
        {
            GameObject shopPanelPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ShopPanel" + ".prefab");
            GameObject shopPanelObj = Instantiate(shopPanelPrefab, transform);
            ShopPanel shopPanel = shopPanelObj.GetComponent<ShopPanel>();
            shopPanel.SetUp("MajoShop");
        });

        btn_changeSkill.onClick.AddListener(async () =>
        {
            await UIManager.ShowPanel(ABconfig.GAME_PREFABS + "ChangeSkillPanel");
        });
        btn_girls.ForEach(btn =>
        {
            btn.onClick.AddListener(async () =>
            {
                AffinityPanel affinityPanel = (await UIManager.ShowCommonPanel("AffinityPanel")).GetComponent<AffinityPanel>();
                affinityPanel.SetUp(btn.name);
            });
        });
        btn_girls.ForEach(btn => btn.gameObject.SetActive(false));
        OpenLevel(GameDataManager.SafeRoomLevel);
        txt_socialPoint.text = LanguageManager.GetFormat("T_SocialPoint", GameDataManager.SocialPoint);
    }

    void SetUpDoorSwitchEventTrigger()
    {
        if (doorEventTrigger == null)
        {
            return;
        }

        AddDoorTriggerEntry(EventTriggerType.PointerEnter, _ =>
        {
            if (isDoorOpenedLocked)
            {
                return;
            }

            ChangeDoorState(DoorState.Opening);
        });

        AddDoorTriggerEntry(EventTriggerType.PointerClick, _ =>
        {
            if (isDoorOpenedLocked)
            {
                return;
            }

            isDoorOpenedLocked = true;
            ChangeDoorState(DoorState.Opened);
        });

        AddDoorTriggerEntry(EventTriggerType.PointerExit, _ =>
        {
            if (isDoorOpenedLocked)
            {
                return;
            }

            ChangeDoorState(DoorState.Closed);
        });
    }

    void AddDoorTriggerEntry(EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };
        entry.callback.AddListener(callback);
        doorEventTrigger.triggers.Add(entry);
    }

    void PreLoadAssets()
    {
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ShopPanel" + ".prefab");
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ChangeSkillPanel" + ".prefab");
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "AffinityPanel" + ".prefab");
    }
    void OpenLevel(int level)
    {
        switch (level)
        {
            case 0:
                // 第一關，增加一些裝飾或特效
                //開放獄卒
                btn_girls[0].gameObject.SetActive(true);
                btn_shop.gameObject.SetActive(false);
                break;
            case 1:
                // 第二關，增加更多裝飾或特效 開啟商店
                btn_girls[0].gameObject.SetActive(true);
                btn_girls[1].gameObject.SetActive(true);
                btn_shop.gameObject.SetActive(true);
                break;
            case 2:
                // 第三關，增加更多裝飾或特效 開啟商店
                btn_girls[0].gameObject.SetActive(true);
                btn_girls[1].gameObject.SetActive(true);
                btn_girls[2].gameObject.SetActive(true);
                btn_shop.gameObject.SetActive(true);
                break;
            case 3:
                // 第四關，增加更多裝飾或特效 開啟商店
                btn_girls.ForEach(btn => btn.gameObject.SetActive(true));
                btn_shop.gameObject.SetActive(true);
                break;
            case 100://特殊使用 幻想房
                btn_girls.ForEach(btn => btn.gameObject.SetActive(true));
                btn_shop.gameObject.SetActive(true);
                break;
            default:
                // 其他關卡，根據需要添加不同的裝飾或特效
                break;
        }
    }
    void OnBackButtonClick()
    {
        if (hasDispatchedBackAfterDoorOpened)
        {
            return;
        }

        hasDispatchedBackAfterDoorOpened = true;
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        SaveManager.AutoSave();
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);

        //GameDataManager.SafeRoomLevel=100 todo 多一個gameover結局
        if (GameDataManager.SafeRoomLevel == 100)
        {
            // 處理特殊結局邏輯
            GameDataManager.SafeRoomLevel = 0;//重置等級以免影響正常遊戲流程
        }
    }
    void ChangeDoorState(DoorState state)
    {
        currentDoorState = state;

        if (img_switch == null || spSwitch == null || spSwitch.Length == 0)
        {
            return;
        }

        int spriteIndex = Mathf.Clamp((int)currentDoorState, 0, spSwitch.Length - 1);
        img_switch.sprite = spSwitch[spriteIndex];

        float targetY = GetDoorTargetY(currentDoorState);
        float moveDuration = GetDoorMoveDuration(currentDoorState);
        bool shouldCallBackWhenArrived = currentDoorState == DoorState.Opened;
        StartDoorMove(targetY, moveDuration, shouldCallBackWhenArrived ? OnBackButtonClick : null);
    }

    float GetDoorTargetY(DoorState state)
    {
        switch (state)
        {
            case DoorState.Opening:
                return DoorOpeningY;
            case DoorState.Opened:
                return DoorOpenedY;
            case DoorState.Closed:
            default:
                return DoorClosedY;
        }
    }

    float GetDoorMoveDuration(DoorState state)
    {
        return state == DoorState.Opened ? doorOpenedMoveDuration : doorMoveDuration;
    }

    void StartDoorMove(float targetY, float moveDuration, System.Action onComplete)
    {
        if (img_doorIron == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (doorMoveCoroutine != null)
        {
            StopCoroutine(doorMoveCoroutine);
        }

        doorMoveCoroutine = StartCoroutine(AnimateDoorToY(targetY, moveDuration, onComplete));
    }

    IEnumerator AnimateDoorToY(float targetY, float moveDuration, System.Action onComplete)
    {
        Vector3 startPos = img_doorIron.localPosition;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        if (moveDuration <= 0f)
        {
            img_doorIron.localPosition = endPos;
            doorMoveCoroutine = null;
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            img_doorIron.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        img_doorIron.localPosition = endPos;
        doorMoveCoroutine = null;
        onComplete?.Invoke();
    }
}
