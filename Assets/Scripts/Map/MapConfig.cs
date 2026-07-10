// MapConfig.cs
public static class MapConfig
{
    // 地图顺序配置，想插入只需修改这个列表
    public static readonly string[] MapOrder = new string[]
    {
        "TestMap",             // 测试地图
        "Map_Prologue1",      // 第1张
        "Map_Prologue2",        // 第2张
        "Map_Prologue3",        // 第3张
    };
    //关卡名称
    public static readonly string[] StageNames = new string[]
    {
        "測試關卡",
        "第一關",
        "第二關",
        "東側監獄",
        "第四關",
        "第五關",
    };
    public static string GetMapAddress(int index)
    {
        if (index < 0 || index >= MapOrder.Length)
            return MapOrder[MapOrder.Length - 1]; // 最后一张
        return MapOrder[index];
    }
    // 获取关卡名称
    public static string GetStageName(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= StageNames.Length)
            return StageNames[StageNames.Length - 1]; // 最后一关
        return StageNames[mapIndex];
    }
}