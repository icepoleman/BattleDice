using UnityEngine;

public class AffinityStoryData
{
    public string role;           // 角色ID
    public string storyName;      // 故事名稱
    public string unlockHint;   // 解鎖提示

    public AffinityStoryData(string id, string name, string hint)
    {
        role = id;
        storyName = name;
        unlockHint = hint;
    }   
}
