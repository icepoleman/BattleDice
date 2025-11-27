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
        EventCenter.AddListener(AdvEvent.EVENT_SHAKE_CAMERA, ShakeCamera);
    }
    void OnDestroy()
    {
        StopShake();
        EventCenter.RemoveListener(AdvEvent.EVENT_SHAKE_CAMERA, ShakeCamera);
    }
    // 公開方法供其他腳本調用
    public void ShakeCamera(object[] args)
    {
        if (!isShaking)
        {
            StartCoroutine(Shake(1f, 0.08f));
        }
    }
    
    // 震動協程
    IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            // 隨機調整相機 size，只能縮小不能放大
            float randomOffset = Random.Range(-magnitude, 0);
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
