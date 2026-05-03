using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 右鍵/ESC 關閉彈窗組件 - 添加到任何 GameObject 上，右鍵或 ESC 放開時銷毀該物件
/// </summary>
public class RightClickCloser : MonoBehaviour
{
    private int spawnFrame;

    void Start()
    {
        spawnFrame = Time.frameCount; // 記錄生成幀，避免同幀關閉
    }

    void Update()
    {
        if (Time.frameCount <= spawnFrame) return; // 跳過生成的同一幀

        // 右鍵放開時關閉
        if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Destroy(gameObject);
            return;
        }

        // ESC 放開時關閉
       /* if (Keyboard.current != null && Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Destroy(gameObject);
        }*/
    }
}
