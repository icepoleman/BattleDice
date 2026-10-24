using UnityEngine;
using UnityEngine.EventSystems;

public class BaseMouseEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.92f;
    [SerializeField] private float lerpSpeed = 14f;

    [Header("State")]
    [SerializeField] private bool effectEnabled = true;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceDuration = 0.14f;
    [SerializeField] private float bounceOvershoot = 0.06f;

    [Header("Floating Settings")]
    [SerializeField] private bool floatingEnabled;
    [SerializeField] private float floatingAmplitude = 8f;
    [SerializeField] private float floatingSpeed = 1f;

    private Transform scaleTarget;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Vector2 originalAnchoredPosition;
    private float floatingTime;
    private bool isPointerOver;
    private bool hasAppliedFloatingOffset;
    private bool isPressed;
    private bool isBouncing;
    private Coroutine bounceRoutine;

    public bool EffectEnabled
    {
        get => effectEnabled;
        set => SetEffectEnabled(value);
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform == null)
        {
            Debug.LogWarning($"{nameof(BaseMouseEffect)} should be used on UI objects with RectTransform.", this);
        }

        scaleTarget = rectTransform != null ? rectTransform : transform;
        originalScale = scaleTarget.localScale;
        targetScale = originalScale;
        originalAnchoredPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
    }

    private void OnEnable()
    {
        scaleTarget.localScale = originalScale;
        targetScale = originalScale;
        isPressed = false;
        isBouncing = false;
        isPointerOver = false;
        floatingTime = 0f;
        if (hasAppliedFloatingOffset && rectTransform != null)
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.y = originalAnchoredPosition.y;
            rectTransform.anchoredPosition = position;
            hasAppliedFloatingOffset = false;
        }
    }

    private void Update()
    {
        if (!effectEnabled)
        {
            UpdateFloating();
            return;
        }

        if (isPressed && Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }

        if (!isBouncing)
        {
            scaleTarget.localScale = Vector3.Lerp(scaleTarget.localScale, targetScale, Time.deltaTime * lerpSpeed);
        }

        UpdateFloating();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        if (!effectEnabled)
        {
            return;
        }

        if (isPressed)
        {
            return;
        }

        SetTargetScale(hoverScaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        if (!effectEnabled)
        {
            return;
        }

        if (isPressed)
        {
            return;
        }

        SetTargetScale(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!effectEnabled)
        {
            return;
        }

        isPressed = true;
        SetTargetScale(pressedScaleMultiplier);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!effectEnabled)
        {
            return;
        }

        HandleMouseUp();
    }

    private void HandleMouseUp()
    {
        if (!isPressed)
        {
            return;
        }

        isPressed = false;
        StartBounceToOriginal();
    }

    private void StartBounceToOriginal()
    {
        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
        }

        bounceRoutine = StartCoroutine(BounceToOriginal());
    }

    public void SetEffectEnabled(bool enabled)
    {
        if (effectEnabled == enabled)
        {
            return;
        }

        effectEnabled = enabled;

        if (!effectEnabled)
        {
            StopBounce();
            isPressed = false;
            targetScale = originalScale;
            scaleTarget.localScale = originalScale;
        }
    }

    private void StopBounce()
    {
        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
            bounceRoutine = null;
        }

        isBouncing = false;
    }

    private System.Collections.IEnumerator BounceToOriginal()
    {
        isBouncing = true;

        Vector3 from = scaleTarget.localScale;
        Vector3 overshoot = originalScale * (1f + bounceOvershoot);

        float half = Mathf.Max(0.01f, bounceDuration * 0.5f);
        float timer = 0f;

        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / half);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            scaleTarget.localScale = Vector3.LerpUnclamped(from, overshoot, eased);
            yield return null;
        }

        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / half);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            scaleTarget.localScale = Vector3.LerpUnclamped(overshoot, originalScale, eased);
            yield return null;
        }

        scaleTarget.localScale = originalScale;
        targetScale = originalScale;
        isBouncing = false;
        bounceRoutine = null;
    }

    private void SetTargetScale(float multiplier)
    {
        targetScale = originalScale * multiplier;
    }

    private void UpdateFloating()
    {
        if (rectTransform == null)
        {
            return;
        }

        if (!floatingEnabled)
        {
            if (hasAppliedFloatingOffset)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                pos.y = originalAnchoredPosition.y;
                rectTransform.anchoredPosition = pos;
                hasAppliedFloatingOffset = false;
            }

            return;
        }

        if (!isPointerOver)
        {
            floatingTime += Time.deltaTime;
        }

        Vector2 position = rectTransform.anchoredPosition;
        position.y = originalAnchoredPosition.y + Mathf.Sin(floatingTime * floatingSpeed) * floatingAmplitude;
        rectTransform.anchoredPosition = position;
        hasAppliedFloatingOffset = true;
    }
}
