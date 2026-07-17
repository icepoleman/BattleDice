using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMouseOutline : MonoBehaviour
{
    [SerializeField] Material outline;
    [SerializeField] bool effectEnabled = true;
    EventTrigger eventTrigger;
    Image self_img;
    Material originalMaterial;

    public bool EffectEnabled
    {
        get => effectEnabled;
        set => SetEffectEnabled(value);
    }

    void Start()
    {
        self_img = GetComponent<Image>();
        eventTrigger = GetComponent<EventTrigger>();

        if (self_img == null)
        {
            return;
        }

        originalMaterial = self_img.material;

        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        AddTrigger(EventTriggerType.PointerEnter, _ =>
        {
            if (!effectEnabled)
            {
                return;
            }

            self_img.material = outline;
        });

        AddTrigger(EventTriggerType.PointerExit, _ =>
        {
            if (!effectEnabled)
            {
                return;
            }

            self_img.material = null;
        });

        if (!effectEnabled)
        {
            self_img.material = originalMaterial;
        }
    }

    public void SetEffectEnabled(bool enabled)
    {
        if (effectEnabled == enabled)
        {
            return;
        }

        effectEnabled = enabled;

        if (self_img == null)
        {
            return;
        }

        if (!effectEnabled)
        {
            self_img.material = originalMaterial;
        }
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
