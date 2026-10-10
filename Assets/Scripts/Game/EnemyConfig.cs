using System;
using System.Collections.Generic;

public class EnemyConfig
{
    public static EnemyConfig Instance { get; private set; } = new EnemyConfig();

    // chapterId -> 這個 chapter 會出現的敵人 ID 列表
    public Dictionary<int, List<int>> ZakoStageEnemyMap { get; } = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> EritoStageEnemyMap { get; } = new Dictionary<int, List<int>>();

    private EnemyConfig()
    {
        InitSampleData();
    }

    public void InitSampleData()
    {
        ZakoStageEnemyMap.Clear();
        EritoStageEnemyMap.Clear();

        // 假資料：Zako 類型
        ZakoStageEnemyMap[1] = new List<int> { 1, 4 };
        ZakoStageEnemyMap[2] = new List<int> { };
        ZakoStageEnemyMap[3] = new List<int> { };
        ZakoStageEnemyMap[4] = new List<int> { };
        ZakoStageEnemyMap[5] = new List<int> { 15, 16 };
        ZakoStageEnemyMap[6] = new List<int> { 12 };

        // 假資料：Erito 類型
        EritoStageEnemyMap[1] = new List<int> { 3 };
        EritoStageEnemyMap[2] = new List<int> { 5 };
        EritoStageEnemyMap[3] = new List<int> { 7 };
        EritoStageEnemyMap[4] = new List<int> { 10, 18 };
        EritoStageEnemyMap[5] = new List<int> { 14 };
        EritoStageEnemyMap[6] = new List<int> { };
    }

    public List<int> GetEnemyIDs(string enemyType, int chapterId)
    {
        Dictionary<int, List<int>> targetMap = null;

        if (string.Equals(enemyType, "Zako", StringComparison.OrdinalIgnoreCase))
            targetMap = ZakoStageEnemyMap;
        else if (string.Equals(enemyType, "Erito", StringComparison.OrdinalIgnoreCase))
            targetMap = EritoStageEnemyMap;

        if (targetMap == null)
            return new List<int>();

        var result = new List<int>();

        for (int currentChapter = 1; currentChapter <= chapterId; currentChapter++)
        {
            if (targetMap.TryGetValue(currentChapter, out var ids))
            {
                result.AddRange(ids);
            }
        }

        return result;
    }
}
