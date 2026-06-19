using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMouseOutline : MonoBehaviour
{
    [SerializeField] Material outline;
    EventTrigger eventTrigger;
    Image self_img;

    void Start()
    {
        self_img = GetComponent<Image>();
        eventTrigger = GetComponent<EventTrigger>();

        if (self_img == null)
        {
            return;
        }

        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        AddTrigger(EventTriggerType.PointerEnter, _ =>
        {
            self_img.material = outline;
        });

        AddTrigger(EventTriggerType.PointerExit, _ =>
        {
            self_img.material = null;
        });
    }

    void AddTrigger(EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = type
        };
        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }
}
