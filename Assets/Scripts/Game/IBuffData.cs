using UnityEngine;

public interface IBuffData
{
    int buffID { get; set; }
    string buffName { get; set; }
    string effectText { get; set; } // Buff 效果描述
    float duration { get; set; } // 持續時間（回合數）
    
    void ApplyBuff(GameObject target);
    void RemoveBuff(GameObject target);
}
