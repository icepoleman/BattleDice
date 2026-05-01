using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadPanel : MonoBehaviour
{
    public enum PanelType
    {
        Save,
        Load
    }
    [SerializeField] GameObject saveItemPrefab;
    [SerializeField] Transform contentTransform;
    [SerializeField] Button btn_close;
    [SerializeField] Button btn_close_black;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetUp(PanelType panelType)
    {
        btn_close.onClick.AddListener(() => { Destroy(gameObject); });
        btn_close_black.onClick.AddListener(() => { Destroy(gameObject); });

        saveItemPrefab.SetActive(false);

        // Load 模式時，先顯示自動存檔
        if (panelType == PanelType.Load)
        {
            SaveSlotInfo autoSaveInfo = SaveManager.GetAutoSaveInfo();
            if (autoSaveInfo != null)
            {
                CreateSaveItem(autoSaveInfo, isAutoSave: true, panelType: panelType);
            }
        }

        // 顯示所有手動存檔槽
        SaveSlotInfo[] saveInfos = SaveManager.GetAllSlotInfos();
        foreach (var info in saveInfos)
        {
            CreateSaveItem(info, isAutoSave: false, panelType: panelType);
        }
    }

    async Task CreateSaveItem(SaveSlotInfo info, bool isAutoSave, PanelType panelType = PanelType.Load)
    {
        GameObject itemObj = Instantiate(saveItemPrefab, contentTransform);
        SaveItemView saveItem = itemObj.GetComponent<SaveItemView>();
        Button itemBtn = itemObj.GetComponent<Button>();
        GameObject confirmPanel = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "ConfirmPanel" + ".prefab");
        itemBtn.onClick.AddListener(async () =>
        {
            if (panelType == PanelType.Save)
            {
                if (!info.isEmpty)
                {
                    Instantiate(confirmPanel, transform).GetComponent<ConfirmPanel>().SetUp(
                        LanguageManager.GetText("T_Save_OverWrite_Hint"), () =>
                        {
                            Debug.Log($"存檔到槽 {info.slotIndex + 1}");
                            SaveManager.SaveToSlot(info.slotIndex);
                            saveItem.SetData(SaveManager.GetSlotInfo(info.slotIndex));
                        });
                    return;
                }
                Debug.Log($"存檔到槽 {info.slotIndex + 1}");
                SaveManager.SaveToSlot(info.slotIndex);
                saveItem.SetData(SaveManager.GetSlotInfo(info.slotIndex));
            }
            else if (panelType == PanelType.Load)
            {
                if (isAutoSave)
                {
                    Debug.Log("載入自動存檔");
                    SaveManager.LoadAutoSave();
                }
                else
                {
                    Debug.Log($"載入存檔槽 {info.slotIndex + 1}");
                    SaveManager.LoadFromSlot(info.slotIndex);
                }
                Destroy(gameObject);
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
            }
        });

        itemBtn.interactable = !info.isEmpty || panelType == PanelType.Save;
        saveItem.SetData(info, isAutoSave);
        itemObj.SetActive(true);
    }
}
