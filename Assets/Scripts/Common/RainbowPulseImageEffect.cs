using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RainbowPulseImageEffect : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float pulseAmount = 0.16f;
    [SerializeField] private float rainbowSpeed = 0.2f;
    [SerializeField] private float whiteBlend = 0.35f;

    private Image targetImage;
    private Text targetText;
    private TMP_Text targetTmpText;
    private Color baseColor = Color.white;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        targetText = GetComponent<Text>();
        targetTmpText = GetComponent<TMP_Text>();

        if (targetImage != null)
        {
            baseColor = targetImage.color;
        }
        else if (targetText != null)
        {
            baseColor = targetText.color;
        }
        else if (targetTmpText != null)
        {
            baseColor = targetTmpText.color;
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        if (targetImage == null && targetText == null && targetTmpText == null)
        {
            return;
        }

        if (targetImage != null && !targetImage.enabled)
        {
            return;
        }

        if (targetText != null && !targetText.enabled)
        {
            return;
        }

        if (targetTmpText != null && !targetTmpText.enabled)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);
        Color rainbow = Color.HSVToRGB(hue, 0.85f, 1f);
        Color animatedColor = Color.Lerp(rainbow, Color.white, whiteBlend);
        animatedColor.a = baseColor.a;

        Color appliedColor = animatedColor * pulse;

        if (targetImage != null)
        {
            targetImage.color = appliedColor;
        }

        if (targetText != null)
        {
            targetText.color = appliedColor;
        }

        if (targetTmpText != null)
        {
            targetTmpText.color = appliedColor;
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            RestoreBaseColor();
        }
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;
        RestoreBaseColor();
    }

    private void RestoreBaseColor()
    {
        if (targetImage != null)
        {
            targetImage.color = baseColor;
        }

        if (targetText != null)
        {
            targetText.color = baseColor;
        }

        if (targetTmpText != null)
        {
            targetTmpText.color = baseColor;
        }
    }
}
