using UnityEngine;
using UnityEngine.UI;

public class SaveLoadPanel : MonoBehaviour
{
    public enum PanelType
    {
        Save,
        Load
    }
    [SerializeField] PanelType panelType;
    [SerializeField] GameObject saveItemPrefab;
    [SerializeField] Transform contentTransform;
    [SerializeField] Button btn_close;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        btn_close.onClick.AddListener(() => { Destroy(gameObject); });
        saveItemPrefab.SetActive(false);
        
        GameObject confirmPanel = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "ConfirmPanel" + ".prefab");

        // Load 模式時，先顯示自動存檔
        if (panelType == PanelType.Load)
        {
            SaveSlotInfo autoSaveInfo = SaveManager.GetAutoSaveInfo();
            if (autoSaveInfo != null)
            {
                CreateSaveItem(autoSaveInfo, confirmPanel, isAutoSave: true);
            }
        }

        // 顯示所有手動存檔槽
        SaveSlotInfo[] saveInfos = SaveManager.GetAllSlotInfos();
        foreach (var info in saveInfos)
        {
            CreateSaveItem(info, confirmPanel, isAutoSave: false);
        }
    }

    void CreateSaveItem(SaveSlotInfo info, GameObject confirmPanel, bool isAutoSave)
    {
        GameObject itemObj = Instantiate(saveItemPrefab, contentTransform);
        SaveItemView saveItem = itemObj.GetComponent<SaveItemView>();
        Button itemBtn = itemObj.GetComponent<Button>();

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
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
            }
        });

        itemBtn.interactable = !info.isEmpty || panelType == PanelType.Save;
        saveItem.SetData(info, isAutoSave);
        itemObj.SetActive(true);
    }
}
