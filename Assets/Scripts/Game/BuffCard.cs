using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffCard : MonoBehaviour
{
    [SerializeField] GameObject obj_infoPanel;
    [SerializeField] Text txt_duration;
    [SerializeField] Text txt_effect;
    EventTrigger eventTrigger;
    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();
        // 添加滑鼠進入事件
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnMouseEnter(); });
        eventTrigger.triggers.Add(entryEnter);

        // 添加滑鼠離開事件
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnMouseExit(); });
        eventTrigger.triggers.Add(entryExit);
    }
    public void SetBuffInfo(IBuffData buffData)
    {
       /* RectTransform rect = GetComponent<RectTransform>();
        if (rect.anchoredPosition.x > 0)
            obj_infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(20, -12, 1);
        else
            obj_infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(-20, -12, 1);*/

        if (buffData.duration > 0)
            txt_duration.text = buffData.duration.ToString();
        txt_effect.text = LanguageManager.GetFormat("T_Buff_Effect_Describe", buffData.buffName, buffData.describe);
        if (buffData.usageCount > 0)
            txt_effect.text += "\n" + LanguageManager.GetFormat("T_Buff_Effect_UsageCount", buffData.usageCount);
    }
    void OnMouseEnter()
    {
        obj_infoPanel.SetActive(true);
    }
    void OnMouseExit()
    {
        obj_infoPanel.SetActive(false);
    }
}
