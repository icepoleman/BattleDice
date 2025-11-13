using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Camera mainCamera;
    float originalSize;
    bool isShaking = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        originalSize = mainCamera.orthographicSize;
        EventCenter.AddListener(MapEvent.EVENT_SHAKE_CAMERA, ShakeCamera);
    }
    void OnDestroy()
    {
        StopShake();
        EventCenter.RemoveListener(MapEvent.EVENT_SHAKE_CAMERA, ShakeCamera);
    }
    // 公開方法供其他腳本調用
    public void ShakeCamera(object[] args)
    {
        if (!isShaking)
        {
            StartCoroutine(Shake(1.5f, 0.2f));
        }
    }
    
    // 震動協程
    IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            // 隨機調整相機 size，在原始 size 基礎上加減 magnitude
            float randomOffset = Random.Range(-magnitude, magnitude);
            mainCamera.orthographicSize = originalSize + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 震動結束後恢復原始 size
        mainCamera.orthographicSize = originalSize;
        isShaking = false;
    }
    
    // 強制停止震動並恢復原始 size
    void StopShake()
    {
        StopAllCoroutines();
        mainCamera.orthographicSize = originalSize;
        isShaking = false;
    }
}
