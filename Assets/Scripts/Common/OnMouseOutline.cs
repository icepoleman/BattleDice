using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMouseOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Material outline;
    [SerializeField] bool effectEnabled = true;
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

        if (self_img == null)
        {
            return;
        }

        originalMaterial = self_img.material;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (effectEnabled && self_img != null)
        {
            self_img.material = outline;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (effectEnabled && self_img != null)
        {
            self_img.material = null;
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
}
