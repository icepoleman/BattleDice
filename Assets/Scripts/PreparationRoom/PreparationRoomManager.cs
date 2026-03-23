using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PreparationRoomManager : MonoBehaviour
{
    [SerializeField] Button btn_back;
    [SerializeField] Button btn_powerUp;
    [SerializeField] GameObject powerUpPanelPrefab;
    [SerializeField] Button btn_shop;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] List<Button> btn_girls; // 角色模型列表，根據角色顯示對應模型
    async Task Start()
    {
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
}
