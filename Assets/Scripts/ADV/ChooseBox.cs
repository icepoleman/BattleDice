using UnityEngine;

public class ChooseBox : MonoBehaviour
{
    [SerializeField]
    private GameObject chooseBtnPrefab;
    public void CreateChooseBtns(string[] btnText, string[] targetTag)
    {
        if (btnText.Length != targetTag.Length)
        {
            Debug.LogError("❌ CreateChooseBtns: btnText 和 targetTag 長度不一致");
            return;
        }
        
        for (int i = 0; i < btnText.Length; i++)
        {
            int index = i; // 捕獲當前的索引值
            GameObject btn = Instantiate(chooseBtnPrefab, transform);
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = btnText[i];
            //設定按鈕點擊事件
            btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
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

}
