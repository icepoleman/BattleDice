using UnityEngine;

public class ChooseBox : MonoBehaviour
{
    [SerializeField]
    private GameObject chooseBtnPrefab;
    public void CreateChooseBtn(string btnText, int _index,System.Action<int> OnChoose)
    {
        GameObject btn = Instantiate(chooseBtnPrefab, transform);
        btn.GetComponentInChildren<TMPro.TMP_Text>().text = btnText;
        //設定按鈕點擊事件
        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnChoose(_index));
    }
    public void ClearChooseBtn()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}
