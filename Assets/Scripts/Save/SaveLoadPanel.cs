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
        //todo 生成列表 修改特定存檔槽顯示
        SaveSlotInfo[] saveInfos = SaveManager.GetAllSlotInfos();
        GameObject confirmPanel = await AddressableManager.LoadAssetAsync<GameObject>( ABconfig.COMMON_PREFABS + "ConfirmPanel" + ".prefab");
        foreach (var info in saveInfos)
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
                                // 點擊後存檔
                                Debug.Log($"存檔到槽 {info.slotIndex + 1}");
                                SaveManager.SaveToSlot(info.slotIndex);//存檔到指定槽位
                                //更新顯示
                                saveItem.SetData(SaveManager.GetSlotInfo(info.slotIndex));
                            });
                        return;
                    }
                    // 點擊後存檔
                    Debug.Log($"存檔到槽 {info.slotIndex + 1}");
                    SaveManager.SaveToSlot(info.slotIndex);//存檔到指定槽位
                    //更新顯示
                    saveItem.SetData(SaveManager.GetSlotInfo(info.slotIndex));
                }
                else if (panelType == PanelType.Load)
                {
                    // 點擊後載入存檔
                    Debug.Log($"載入存檔槽 {info.slotIndex + 1}");
                    SaveManager.LoadFromSlot(info.slotIndex);//載入自動存檔
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
                }
            });
            itemBtn.interactable = !info.isEmpty || panelType == PanelType.Save;
            saveItem.SetData(info);
            itemObj.SetActive(true);
        }
    }
}
