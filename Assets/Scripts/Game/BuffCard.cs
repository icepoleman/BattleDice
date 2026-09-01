using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffCard : MonoBehaviour
{
    [SerializeField] GameObject obj_infoPanel;
    [SerializeField] Text txt_duration;
    [SerializeField] Text txt_effect;
    [SerializeField] Image img_buffIcon;
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
        img_buffIcon.sprite = AtlasLoader.Instance.GetBuffSprite(buffData.buffID.ToString());

        if (buffData.duration > 0)
            txt_duration.text = buffData.duration.ToString();

        txt_effect.text = LanguageManager.GetFormat("T_Buff_Effect_Describe", buffData.buffName, buffData.describe);
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
