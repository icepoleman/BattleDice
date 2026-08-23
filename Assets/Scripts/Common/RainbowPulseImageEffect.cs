using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RainbowPulseImageEffect : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float pulseAmount = 0.16f;
    [SerializeField] private float rainbowSpeed = 0.2f;
    [SerializeField] private float whiteBlend = 0.35f;

    private Image targetImage;
    private Color baseColor = Color.white;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            baseColor = targetImage.color;
        }
    }

    private void Update()
    {
        if (!isActive || targetImage == null || !targetImage.enabled)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);
        Color rainbow = Color.HSVToRGB(hue, 0.85f, 1f);
        Color animatedColor = Color.Lerp(rainbow, Color.white, whiteBlend);
        animatedColor.a = baseColor.a;

        targetImage.color = animatedColor * pulse;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive && targetImage != null)
        {
            targetImage.color = baseColor;
        }
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;
        if (targetImage != null)
        {
            targetImage.color = color;
        }
    }
}
