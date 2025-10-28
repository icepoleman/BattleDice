using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(string sceneName, System.Action onComplete = null)
    {
        // 顯示載入畫面
        ShowLoadingScreen();
        
        // 異步載入場景
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += (op) =>
        {
            HideLoadingScreen();
            onComplete?.Invoke();
            Debug.Log($"場景 {sceneName} 載入完成");
        };
    }
    
    static void ShowLoadingScreen()
    {
        // TODO: 顯示載入畫面
        Debug.Log("顯示載入畫面");
    }
    
    static void HideLoadingScreen()
    {
        // TODO: 隱藏載入畫面
        Debug.Log("隱藏載入畫面");
    }
}