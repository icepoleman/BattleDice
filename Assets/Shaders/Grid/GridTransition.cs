using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GridTransition : MonoBehaviour
{
    private static readonly int StepId = Shader.PropertyToID("_Step");
    private static readonly int FadeInId = Shader.PropertyToID("_FadeIn");

    [Header("References")]
    [SerializeField] private Material material;
    [SerializeField] private Graphic targetGraphic;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool enableSpaceKeyTest = true;

    private Material runtimeMaterial;
    private Coroutine playingCoroutine;
    private bool isCovered;

    private void Reset()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    private void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        EnsureRuntimeMaterial();
        SetInstantTransparent();
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }

    private void Update()
    {
        if (!enableSpaceKeyTest || !Input.GetKeyDown(KeyCode.Space))
            return;

        if (isCovered)
            FadeIn();
        else
            FadeOut();
    }

    public void FadeOut(Action onComplete = null)
    {
        FadeOut(fadeOutDuration, onComplete);
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        PlayTransition(isFadeIn: false, duration, onComplete);
    }

    public void FadeIn(Action onComplete = null)
    {
        FadeIn(fadeInDuration, onComplete);
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        PlayTransition(isFadeIn: true, duration, onComplete);
    }

    public void SetInstantCovered()
    {
        StopCurrentAnimation();
        gameObject.SetActive(true);
        ApplyShader(isFadeIn: false, step: 1f);
        isCovered = true;
    }

    public void SetInstantTransparent()
    {
        StopCurrentAnimation();
        ApplyShader(isFadeIn: true, step: 1f);
        isCovered = false;
    }

    private void PlayTransition(bool isFadeIn, float duration, Action onComplete)
    {
        if (runtimeMaterial == null)
        {
            EnsureRuntimeMaterial();
            if (runtimeMaterial == null)
            {
                Debug.LogError("[GridTransition] Material is missing.");
                onComplete?.Invoke();
                return;
            }
        }

        StopCurrentAnimation();
        gameObject.SetActive(true);
        playingCoroutine = StartCoroutine(PlayTransitionRoutine(isFadeIn, duration, onComplete));
    }

    private IEnumerator PlayTransitionRoutine(bool isFadeIn, float duration, Action onComplete)
    {
        ApplyShader(isFadeIn, 0f);

        if (duration <= 0f)
        {
            ApplyShader(isFadeIn, 1f);
            isCovered = !isFadeIn;
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
            ApplyShader(isFadeIn, easedT);
            yield return null;
        }

        ApplyShader(isFadeIn, 1f);
        isCovered = !isFadeIn;
        onComplete?.Invoke();
        playingCoroutine = null;
    }

    private void ApplyShader(bool isFadeIn, float step)
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetFloat(FadeInId, isFadeIn ? 1f : 0f);
        runtimeMaterial.SetFloat(StepId, Mathf.Clamp01(step));
    }

    private void EnsureRuntimeMaterial()
    {
        if (runtimeMaterial != null)
            return;

        Material source = material;
        if (source == null && targetGraphic != null)
            source = targetGraphic.material;

        if (source == null)
            return;

        runtimeMaterial = new Material(source);
        runtimeMaterial.name = source.name + " (Runtime)";

        if (targetGraphic != null)
            targetGraphic.material = runtimeMaterial;
    }

    private void StopCurrentAnimation()
    {
        if (playingCoroutine == null)
            return;

        StopCoroutine(playingCoroutine);
        playingCoroutine = null;
    }
}
