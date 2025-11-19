using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static MonoBehaviour coroutineRunner;
    
    // 設定協程執行者（通常在遊戲啟動時設定）
    public static void SetCoroutineRunner(MonoBehaviour runner)
    {
        coroutineRunner = runner;
    }
    
    // 帶延遲的場景載入（推薦使用）
    public static void LoadSceneWithDelay(string sceneName, System.Action onComplete = null)
    {
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
        }
        else
        {
            Debug.LogWarning("CoroutineRunner 未設定，使用同步載入");
            LoadScene(sceneName, onComplete);
        }
    }
    
    // 載入協程
    static IEnumerator LoadSceneCoroutine(string sceneName, System.Action onComplete)
    {
        // 先顯示載入畫面
        ShowLoadingScreen();
        
        // 等待淡入動畫播完 (0.5秒)
        yield return new WaitForSeconds(0.6f);
        
        // 異步載入場景
        Debug.Log($"開始載入場景: {sceneName}");
        var operation = SceneManager.LoadSceneAsync(sceneName);
        
        // 等待場景載入完成
        while (!operation.isDone)
        {
            yield return null;
        }
        
        Debug.Log($"場景 {sceneName} 載入完成");
        
        // 額外等待確保場景完全初始化
        yield return new WaitForSeconds(0.3f);
        
        // 隱藏載入畫面
        HideLoadingScreen();
        
        // 執行回調
        onComplete?.Invoke();
    }
    
    // 原始的立即載入方法（保持向後兼容）
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
        Debug.Log("顯示載入畫面");
        EventCenter.Dispatch(StateEvent.EVENT_LOADING_SCREEN, true);
    }
    
    static void HideLoadingScreen()
    {
        Debug.Log("隱藏載入畫面");
        EventCenter.Dispatch(StateEvent.EVENT_LOADING_SCREEN, false);
    }
}