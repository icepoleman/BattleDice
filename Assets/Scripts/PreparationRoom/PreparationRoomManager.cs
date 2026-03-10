using UnityEngine;
using UnityEngine.UI;

public class PreparationRoomManager : MonoBehaviour
{
    [SerializeField] Button btn_back;
    [SerializeField] Button btn_powerUp;
    [SerializeField] GameObject powerUpPanelPrefab;
    [SerializeField] Button btn_shop;
    [SerializeField] Button btn_changeSkill;
    void Start()
    {
        btn_back.onClick.AddListener(OnBackButtonClick);
        btn_powerUp.onClick.AddListener(() =>
        {
            Instantiate(powerUpPanelPrefab, transform);
        });
        if (GameDataManager.CurrentMap > 2)
        {
            btn_shop.onClick.AddListener(async () =>
            {
            GameObject shopPanelPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "ShopPanel" + ".prefab");
            GameObject shopPanelObj = Instantiate(shopPanelPrefab, transform);
            ShopPanel shopPanel = shopPanelObj.GetComponent<ShopPanel>();
            shopPanel.SetUp("MajoShop");
            });
        }
        else
        {
            btn_shop.gameObject.SetActive(false);
        }

        btn_changeSkill.onClick.AddListener(async () =>
        {
            await UIManager.ShowPanel(ABconfig.GAME_PREFABS + "ChangeSkillPanel");
        });
    }
    void OnBackButtonClick()
    {
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        SaveManager.AutoSave();
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
    }
}
