using UnityEngine;
using UnityEngine.UI;

public class ChooseBox : MonoBehaviour
{
    [SerializeField] GameObject chooseBtnPrefab;

    public void CreateChooseBtns(string[] btnText, string[] targetTag)
    {
        if (btnText.Length != targetTag.Length)
        {
            Debug.LogError("❌ CreateChooseBtns: btnText 和 targetTag 長度不一致");
            return;
        }

        if (chooseBtnPrefab == null)
        {
            Debug.LogError("❌ btn_choose 尚未載入");
            return;
        }

        for (int i = 0; i < btnText.Length; i++)
        {
            int index = i; // 捕獲當前的索引值
            string choiceText = btnText[i];
            GameObject btn = Instantiate(chooseBtnPrefab, transform);
            btn.GetComponentInChildren<Text>().text = choiceText;
            //設定按鈕點擊事件
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                EventCenter.Dispatch(AdvEvent.EVENT_CLICK_CHOICE, targetTag[index], choiceText);
                ClearChooseBtn();
            });
            btn.gameObject.SetActive(true);
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
