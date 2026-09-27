using System;
using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private Coroutine shakeRoutine;
    private Vector3 originalLocalPosition;
    private Vector2 originalAnchoredPosition;
    private RectTransform rectTransform;

    private void Awake()
    {
        CacheOriginalPosition();
    }

    private void OnEnable()
    {
        EventCenter.AddListener(AdvEvent.EVENT_SHAKE_SCREEN, OnShakeScreenEvent);
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener(AdvEvent.EVENT_SHAKE_SCREEN, OnShakeScreenEvent);
        ResetPosition();
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener(AdvEvent.EVENT_SHAKE_SCREEN, OnShakeScreenEvent);
    }

    private void CacheOriginalPosition()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }
        else
        {
            originalLocalPosition = transform.localPosition;
        }
    }

    private void OnShakeScreenEvent(object[] args)
    {
        if (args == null || args.Length < 2)
        {
            return;
        }

        float duration = args[0] is float f ? f : Convert.ToSingle(args[0]);
        float strength = args[1] is float s ? s : Convert.ToSingle(args[1]);
        TriggerShake(duration, strength);
    }

    public void TriggerShake(float duration, float strength)
    {
        if (duration <= 0f || strength <= 0f)
        {
            return;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        CacheOriginalPosition();
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float decay = Mathf.Clamp01(1f - (elapsed / duration));
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * strength * decay;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition + randomOffset;
            }
            else
            {
                transform.localPosition = originalLocalPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetPosition();
        shakeRoutine = null;
    }

    private void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            transform.localPosition = originalLocalPosition;
        }
    }
}
