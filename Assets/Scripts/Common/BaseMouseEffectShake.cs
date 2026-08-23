using UnityEngine;
using UnityEngine.EventSystems;

public class BaseMouseEffectShake : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float shakeAngle = 8f;
    [SerializeField] float shakeDuration = 0.18f;

    Transform rotateTarget;
    Quaternion originalRotation;
    Coroutine shakeRoutine;

    void Awake()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null)
        {
            Debug.LogWarning($"{nameof(BaseMouseEffectShake)} should be used on UI objects with RectTransform.", this);
        }

        rotateTarget = rectTransform != null ? rectTransform : transform;
        originalRotation = rotateTarget.localRotation;
    }

    void OnEnable()
    {
        rotateTarget.localRotation = originalRotation;
        StopShake();
    }

    void OnDisable()
    {
        StopShake();
        rotateTarget.localRotation = originalRotation;
    }

    public void OnPointerEnter(PointerEventData eventData) => StartShake();

    public void OnPointerExit(PointerEventData eventData) { }

    public void OnPointerDown(PointerEventData eventData) => StartShake();

    public void OnPointerUp(PointerEventData eventData) { }

    public void StartShake()
    {
        if (shakeRoutine != null)
        {
            return;
        }

        shakeRoutine = StartCoroutine(ShakeOnce());
    }

    void StopShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
    }

    System.Collections.IEnumerator ShakeOnce()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float z = Mathf.Sin(t * Mathf.PI * 4f) * shakeAngle * (1f - t);
            rotateTarget.localRotation = originalRotation * Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        rotateTarget.localRotation = originalRotation;
        shakeRoutine = null;
    }
}
