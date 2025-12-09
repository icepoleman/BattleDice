using UnityEngine;
using UnityEngine.UI;

public class ChooseBox : MonoBehaviour
{
    void Start()
    {
        // 使用新的 AddressableManager
        AddressableManager.LoadAssetAsync<GameObject>("btn_choose");
    }
    public void CreateChooseBtns(string[] btnText, string[] targetTag)
    {
        if (btnText.Length != targetTag.Length)
        {
            Debug.LogError("❌ CreateChooseBtns: btnText 和 targetTag 長度不一致");
            return;
        }

        // 獲取已載入的按鈕預置物
        GameObject btnPrefab = AddressableManager.GetLoadedAsset<GameObject>("btn_choose");
        if (btnPrefab == null)
        {
            Debug.LogError("❌ btn_choose 尚未載入");
            return;
        }

        for (int i = 0; i < btnText.Length; i++)
        {
            int index = i; // 捕獲當前的索引值
            GameObject btn = Instantiate(btnPrefab, transform);
            btn.GetComponentInChildren<Text>().text = btnText[i];
            //設定按鈕點擊事件
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                EventCenter.Dispatch(AdvEvent.EVENT_CLICK_CHOICE, targetTag[index]);
                ClearChooseBtn();
            });
        }
    }

    void ClearChooseBtn()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    void OnDestroy()
    {
        // AddressableManager 會自動管理資源釋放
        // 如果需要立即釋放，可以調用：
        // AddressableManager.ReleaseAsset("btn_choose");
    }
}
