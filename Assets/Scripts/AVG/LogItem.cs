using UnityEngine;
using UnityEngine.UI;

public class LogItem : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI nameText; // 角色名稱文字
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText; // 對話文本
    public void SetData(string name, string dialogue)
    {
        // 根據 DialogueData 設置日誌項目的顯示內容
        // 例如：設置角色名稱、對話文本等
        nameText.text = name;
        dialogueText.text = dialogue;
    }
}
