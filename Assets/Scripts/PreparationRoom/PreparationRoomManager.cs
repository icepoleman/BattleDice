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
        btn_shop.onClick.AddListener(async () =>
        {
            GameObject shopPanelPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "ShopPanel" + ".prefab");
            GameObject shopPanelObj = Instantiate(shopPanelPrefab, transform);
            ShopPanel shopPanel = shopPanelObj.GetComponent<ShopPanel>();
            shopPanel.SetUp("MajoShop");
        });
        btn_changeSkill.onClick.AddListener(async () =>
        {
            GameObject changeSkillViewPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "ChangeSkillView" + ".prefab");
            Instantiate(changeSkillViewPrefab, transform);
        });
    }
    void OnBackButtonClick()
    {
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        SaveManager.AutoSave();
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
    }
}
