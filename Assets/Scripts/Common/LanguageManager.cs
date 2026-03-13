using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 語言管理器 - 讀取多語言文字
/// </summary>
public static class LanguageManager
{
    private static Dictionary<string, string> languageData = new Dictionary<string, string>();
    private static string currentLanguage = "tw";
    private static bool isLoaded = false;
    
    /// <summary>
    /// 當前語言
    /// </summary>
    public static string CurrentLanguage => currentLanguage;
    
    /// <summary>
    /// 設置語言並重新載入
    /// </summary>
    /// <param name="language">語言代碼 (cn, tw, en 等)</param>
    public static void SetLanguage(string language)
    {
        currentLanguage = language;
        isLoaded = false;
        LoadLanguageFile();
    }
    
    /// <summary>
    /// 獲取文字
    /// </summary>
    /// <param name="key">文字鍵值</param>
    /// <returns>對應的文字，如果找不到則返回鍵值</returns>
    public static string GetText(string key)
    {
        if (!isLoaded)
        {
            LoadLanguageFile();
        }
        
        if (languageData.TryGetValue(key, out string value))
        {
            return value;
        }
        
        Debug.LogWarning($"[LanguageManager] 找不到鍵值: {key}");
        return key;
    }
    
    /// <summary>
    /// 獲取格式化文字（支援參數）
    /// </summary>
    /// <param name="key">文字鍵值</param>
    /// <param name="args">格式化參數</param>
    /// <returns>格式化後的文字</returns>
    public static string GetFormat(string key, params object[] args)
    {
        string text = GetText(key);
        
        try
        {
            return string.Format(text, args);
        }
        catch (System.FormatException e)
        {
            Debug.LogError($"[LanguageManager] 格式化失敗: {key} - {e.Message}");
            return text;
        }
    }
    
    /// <summary>
    /// 載入語言檔案
    /// </summary>
    private static void LoadLanguageFile()
    {
        languageData.Clear();
        
        string path = $"Language/{currentLanguage}/{currentLanguage}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        
        if (textAsset == null)
        {
            Debug.LogError($"[LanguageManager] 找不到語言檔案: {path}");
            return;
        }
        
        string[] lines = textAsset.text.Split('\n');
        
        foreach (string line in lines)
        {
            // 跳過空行和註解
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
            {
                continue;
            }
            
            // 分割鍵值對 (格式: key|value)
            int separatorIndex = trimmedLine.IndexOf('|');
            if (separatorIndex > 0)
            {
                string key = trimmedLine.Substring(0, separatorIndex).Trim();
                string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                
                // 處理轉義字符
                value = value.Replace("\\n", "\n");
                value = value.Replace("\\t", "\t");
                
                if (!languageData.ContainsKey(key))
                {
                    languageData[key] = value;
                }
                else
                {
                    Debug.LogWarning($"[LanguageManager] 重複的鍵值: {key}");
                }
            }
        }
        
        isLoaded = true;
        Debug.Log($"[LanguageManager] 載入語言檔案完成: {currentLanguage}，共 {languageData.Count} 條");
    }
    
    /// <summary>
    /// 重新載入當前語言檔案
    /// </summary>
    public static void Reload()
    {
        isLoaded = false;
        LoadLanguageFile();
    }
    
    /// <summary>
    /// 檢查鍵值是否存在
    /// </summary>
    /// <param name="key">文字鍵值</param>
    /// <returns>是否存在</returns>
    public static bool HasKey(string key)
    {
        if (!isLoaded)
        {
            LoadLanguageFile();
        }
        
        return languageData.ContainsKey(key);
    }
}
