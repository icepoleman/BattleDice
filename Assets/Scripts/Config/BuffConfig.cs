using System.Collections.Generic;
public struct BuffSeed
{
    public int buffID;
    public int usageCount;
    public int duration;

    public BuffSeed(int _buffID, int _usageCount, int _duration)
    {
        buffID = _buffID;
        usageCount = _usageCount;
        duration = _duration;
    }
}

// Buff 配置結構體
public struct BuffConfigData
{
    // 基本資訊
    public int buffID;                   // Buff 唯一識別碼
    public string buffName;// Buff 名稱
    public BuffTrigger buffTrigger;
    public BuffEffectType buffEffectType;// 效果類型   
    public int[] effectValues;           // 效果數值列表      
    public string describe;              // Buff 效果描述
}

// Buff 配置資料庫
public static class BuffDatabase
{
    public static HashSet<int> SpBuffIDs = new HashSet<int> {14,17}; // 不堆疊回合的Buff
    private static Dictionary<int, BuffConfigData> _buffs;
    private static bool _isLoaded = false;

    public static Dictionary<int, BuffConfigData> Buffs
    {
        get
        {
            if (!_isLoaded)
            {
                LoadFromCSV();
            }
            return _buffs;
        }
    }

    // 從 CSV 載入
    public static void LoadFromCSV()
    {
        _buffs = CSVReader.Instance.LoadBuffCSV();
        if (_buffs == null)
        {
            _buffs = new Dictionary<int, BuffConfigData>();
            UnityEngine.Debug.LogWarning("⚠️ Buff CSV 載入失敗，使用空資料");
        }
        _isLoaded = true;
    }

    // 重新載入（熱更新用）
    public static void Reload()
    {
        _isLoaded = false;
        LoadFromCSV();
    }

    public static BuffConfigData GetBuffConfig(int buffID)
    {
        if (Buffs.TryGetValue(buffID, out var config))
        {
            return config;
        }
        return default;
    }
}