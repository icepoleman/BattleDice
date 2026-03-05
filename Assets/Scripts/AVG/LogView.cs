using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogView : MonoBehaviour
{
    [SerializeField] GameObject logItemPrefab;
    [SerializeField] Transform trans_logContent;
    [SerializeField] private Button btn_close;
    private List<ChatLog> chatDatas = new List<ChatLog>();
    public void SetData(List<ChatLog> _data)
    {
        chatDatas = _data;
        RefreshView();
        btn_close.onClick.AddListener(() => { Destroy(gameObject); });
    }
    void RefreshView()
    {
        if(chatDatas == null || chatDatas.Count == 0)
            return; // 沒有對話數據，直接返回
        // 清除現有的日誌項目
        foreach (Transform child in trans_logContent)
        {
            Destroy(child.gameObject);
        }
        // 根據 chatDatas 創建新的日誌項目
        foreach (var data in chatDatas)
        {
            GameObject logItemObj = Instantiate(logItemPrefab, trans_logContent);
            LogItem logItem = logItemObj.GetComponent<LogItem>();
            logItem.SetData(data.name, data.dialogue);
            logItemObj.SetActive(true);
        }
    }
}
