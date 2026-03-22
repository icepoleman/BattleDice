using System.Collections.Generic;
using UnityEngine;
public class CharacterAffinityData
{
    public string characterID;  // 角色ID
    public string[] affinityEvents; // 角色親密度事件列表
    public string[] affinityEventNames; // 角色親密度事件名稱列表
    public string[] affinityEventHints;
}
public class CharacterAffinityConfig
{
    public CharacterAffinityData witchAffinityData = new CharacterAffinityData
    {
        characterID = "Witch",
        affinityEvents = new string[]
        {
            "Witch_Affinity_Event1",
            "Witch_Affinity_Event2",
            "Witch_Affinity_Event3"
        },
        affinityEventNames = new string[]
        {
            "女巫的微笑",
            "女巫的禮物",
            "女巫的秘密"
        },
        affinityEventHints = new string[]
        {
            "第一章整備室",
            "購買三個魔法",
            "親密度max"
        }
    };
}
