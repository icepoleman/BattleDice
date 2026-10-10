using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ShaderDataCtrl : MonoBehaviour
{
    [SerializeField] private Graphic shaderTargetGraphic;
    [SerializeField, Range(0f, 1f)] private float currentProgress;
    [SerializeField] private string shaderProgressProperty = "_Progress";

    public float CurrentProgress
    {
        get => currentProgress;
        set => SetShaderProgress(value);
    }

    private Material runtimeMaterial;
    private float lastAppliedProgress = float.NaN;

    private void Awake()
    {
        if (shaderTargetGraphic == null)
            shaderTargetGraphic = GetComponent<Graphic>();

        EnsureRuntimeMaterial();
        SetShaderProgress(currentProgress);
    }

    private void OnValidate()
    {
        if (shaderTargetGraphic == null)
            shaderTargetGraphic = GetComponent<Graphic>();

        if (!Application.isPlaying)
            EnsureRuntimeMaterial();

        SetShaderProgress(currentProgress);
    }

    private void Update()
    {
        if (currentProgress != lastAppliedProgress)
            SetShaderProgress(currentProgress);
    }

    public void SetShaderProgress(float value)
    {
        currentProgress = Mathf.Clamp01(value);
        lastAppliedProgress = currentProgress;

        if (runtimeMaterial != null && runtimeMaterial.HasProperty(shaderProgressProperty))
            runtimeMaterial.SetFloat(shaderProgressProperty, currentProgress);
    }

    private void EnsureRuntimeMaterial()
    {
        if (shaderTargetGraphic == null || shaderTargetGraphic.material == null)
            return;

        if (runtimeMaterial == null)
        {
            runtimeMaterial = new Material(shaderTargetGraphic.material);
            runtimeMaterial.name = shaderTargetGraphic.material.name + " (Runtime)";
            shaderTargetGraphic.material = runtimeMaterial;
        }
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
