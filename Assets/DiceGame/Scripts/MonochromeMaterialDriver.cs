using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class MonochromeMaterialDriver : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField, Range(0f, 1f)] private float preserveWhite = 1f;

    private Material runtimeMaterial;
    private bool ownsRuntimeMaterial;

    private static readonly int PreserveWhiteId = Shader.PropertyToID("_PreserveWhite");

    private void Reset()
    {
        targetGraphic = GetComponent<Graphic>();
        targetRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        EnsureTargetAssigned();
        EnsureRuntimeMaterial();
        ApplyToMaterial();
    }

    private void OnEnable()
    {
        EnsureTargetAssigned();
        EnsureRuntimeMaterial();
        ApplyToMaterial();
    }

    private void OnDestroy()
    {
        ReleaseRuntimeMaterial();
    }

    private void OnValidate()
    {
        preserveWhite = Mathf.Clamp01(preserveWhite);
        EnsureTargetAssigned();
        EnsureRuntimeMaterial();
        ApplyToMaterial();
    }

    private void OnDidApplyAnimationProperties()
    {
        preserveWhite = Mathf.Clamp01(preserveWhite);
        ApplyToMaterial();
    }

    public void SetPreserveWhite(float value)
    {
        preserveWhite = Mathf.Clamp01(value);
        ApplyToMaterial();
    }

    private void EnsureTargetAssigned()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    private void EnsureRuntimeMaterial()
    {
        if (runtimeMaterial != null)
        {
            return;
        }

        Material sourceMaterial = GetSourceMaterial();
        if (sourceMaterial == null)
        {
            return;
        }

        runtimeMaterial = new Material(sourceMaterial)
        {
            name = sourceMaterial.name + " (Monochrome Instance)"
        };
        ownsRuntimeMaterial = true;
        AssignRuntimeMaterial(runtimeMaterial);
    }

    private Material GetSourceMaterial()
    {
        if (targetGraphic != null)
        {
            return targetGraphic.material;
        }

        if (targetRenderer != null)
        {
            return targetRenderer.sharedMaterial;
        }

        return null;
    }

    private void AssignRuntimeMaterial(Material material)
    {
        if (targetGraphic != null)
        {
            targetGraphic.material = material;
        }

        if (targetRenderer != null)
        {
            targetRenderer.sharedMaterial = material;
        }
    }

    private void ApplyToMaterial()
    {
        if (runtimeMaterial == null)
        {
            EnsureRuntimeMaterial();
        }

        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetFloat(PreserveWhiteId, preserveWhite);
    }

    private void ReleaseRuntimeMaterial()
    {
        if (runtimeMaterial == null || !ownsRuntimeMaterial)
        {
            runtimeMaterial = null;
            ownsRuntimeMaterial = false;
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeMaterial);
        }
        else
        {
            DestroyImmediate(runtimeMaterial);
        }

        runtimeMaterial = null;
        ownsRuntimeMaterial = false;
    }
}
