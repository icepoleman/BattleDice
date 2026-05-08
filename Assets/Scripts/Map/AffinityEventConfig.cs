using System.Collections.Generic;
using UnityEngine;

public class RoleEventData
{
    public string[] allSpEventNames;
    public string[] allNormalEventNames;
}

public static class AffinityEventConfig
{
    readonly static Dictionary<string, RoleEventData> _eventDataMap = new Dictionary<string, RoleEventData>
    {
        ["JailerGirl"] = new RoleEventData
        {
            allSpEventNames = new[] { "Affinity_JailerGirl_11" },
            allNormalEventNames = new[] { "Affinity_JailerGirl_1", "Affinity_JailerGirl_2" }
        },
        ["Witch"] = new RoleEventData
        {
            allSpEventNames = new[] { "Affinity_Witch_11" },
            allNormalEventNames = new[] { "Affinity_Witch_1", "Affinity_Witch_2" }
        },
        ["WolfGirl"] = new RoleEventData
        {
            allSpEventNames = new[] { "Affinity_JailerGirl_11" },
            allNormalEventNames = new[] { "Affinity_WolfGirl_1", "Affinity_WolfGirl_2" }
        },
        ["Idol"] = new RoleEventData
        {
            allSpEventNames = new[] { "Affinity_JailerGirl_11" },
            allNormalEventNames = new[] { "Affinity_Idol_1", "Affinity_Idol_2" }
        },
    };

    public static RoleEventData GetRoleEventData(string role)
    {
        if (_eventDataMap.TryGetValue(role, out var data))
            return data;

        Debug.LogWarning($"未知的角色ID: {role}，返回空事件資料");
        return new RoleEventData { allSpEventNames = new string[0], allNormalEventNames = new string[0] };
    }
}
