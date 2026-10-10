using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionAnim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Graphic shaderTargetGraphic;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Scale")]
    [SerializeField] private bool useScaleEffect = true;
    [SerializeField] private float startScale = 1.03f;
    [SerializeField] private float endScale = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableSpaceKeyTest = true;

    [Header("Shader")]
    [SerializeField] private bool useShaderProgress = true;
    [SerializeField] private string shaderProgressProperty = "_Progress";

    private Coroutine playingCoroutine;
    private bool isVisible;
    private Material runtimeMaterial;

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        contentRoot = transform as RectTransform;
        shaderTargetGraphic = GetComponent<Graphic>();
    }

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (contentRoot == null)
            contentRoot = transform as RectTransform;

        if (shaderTargetGraphic == null)
            shaderTargetGraphic = GetComponent<Graphic>();

        EnsureRuntimeMaterial();
    }

    public void SetInstantVisible(bool isVisible)
    {
        StopCurrentAnimation();
        ApplyVisual(isVisible ? 1f : 0f, isVisible ? endScale : startScale);
        canvasGroup.blocksRaycasts = isVisible;
        canvasGroup.interactable = isVisible;
        this.isVisible = isVisible;
    }

    public void PlayIn(Action onComplete = null)
    {
        StopCurrentAnimation();
        playingCoroutine = StartCoroutine(PlayTransitionRoutine(
            fromAlpha: 0f,
            toAlpha: 1f,
            fromScale: startScale,
            toScale: endScale,
            duration: fadeInDuration,
            onComplete: () =>
            {
                isVisible = true;
                onComplete?.Invoke();
            }));
    }

    public void PlayOut(Action onComplete = null)
    {
        StopCurrentAnimation();
        playingCoroutine = StartCoroutine(PlayTransitionRoutine(
            fromAlpha: 1f,
            toAlpha: 0f,
            fromScale: endScale,
            toScale: startScale,
            duration: fadeOutDuration,
            onComplete: () =>
            {
                isVisible = false;
                onComplete?.Invoke();
            }));
    }

    private void Update()
    {
        if (!enableSpaceKeyTest || !Input.GetKeyDown(KeyCode.Space))
            return;

        if (isVisible)
            PlayOut();
        else
            PlayIn();
    }

    private IEnumerator PlayTransitionRoutine(float fromAlpha, float toAlpha, float fromScale, float toScale, float duration, Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        ApplyVisual(fromAlpha, fromScale);

        if (duration <= 0f)
        {
            ApplyVisual(toAlpha, toScale);
            canvasGroup.blocksRaycasts = toAlpha > 0f;
            canvasGroup.interactable = toAlpha > 0f;
            onComplete?.Invoke();
            playingCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, easedT);
            float scale = Mathf.Lerp(fromScale, toScale, easedT);
            ApplyVisual(alpha, scale);
            yield return null;
        }

        ApplyVisual(toAlpha, toScale);
        canvasGroup.blocksRaycasts = toAlpha > 0f;
        canvasGroup.interactable = toAlpha > 0f;
        onComplete?.Invoke();
        playingCoroutine = null;
    }

    private void ApplyVisual(float alpha, float scale)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;

        if (useScaleEffect && contentRoot != null)
            contentRoot.localScale = Vector3.one * scale;

        if (useShaderProgress && runtimeMaterial != null && runtimeMaterial.HasProperty(shaderProgressProperty))
            runtimeMaterial.SetFloat(shaderProgressProperty, alpha);
    }

    private void EnsureRuntimeMaterial()
    {
        if (!useShaderProgress || shaderTargetGraphic == null || shaderTargetGraphic.material == null)
            return;

        runtimeMaterial = new Material(shaderTargetGraphic.material);
        runtimeMaterial.name = shaderTargetGraphic.material.name + " (Runtime)";
        shaderTargetGraphic.material = runtimeMaterial;
    }

    private void StopCurrentAnimation()
    {
        if (playingCoroutine == null)
            return;

        StopCoroutine(playingCoroutine);
        playingCoroutine = null;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
