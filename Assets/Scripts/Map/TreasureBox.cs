using System;
using System.Collections.Generic;
using UnityEngine;

public enum TreasureType
{
    Gold,
    Gear,
    Skill
}

[Serializable]
public class TreasureTypeWeight
{
    public TreasureType type;
    public float weight = 1f;
}

public class TreasureBox : MonoBehaviour
{
    [SerializeField] private List<TreasureTypeWeight> treasureTypeWeights = new List<TreasureTypeWeight>
    {
        new TreasureTypeWeight { type = TreasureType.Gold, weight = 1f },
        new TreasureTypeWeight { type = TreasureType.Gear, weight = 1f },
        new TreasureTypeWeight { type = TreasureType.Skill, weight = 1f },
    };
    [SerializeField] private bool rollOnStart = true;

    public TreasureType CurrentType { get; private set; }

    private void Start()
    {
        if (rollOnStart)
            Reroll();
    }

    /// <summary>依 Inspector 設定的機率重新抽選寶箱類型。</summary>
    public TreasureType Reroll()
    {
        CurrentType = RollTreasureType();
        Debug.LogError($"Rerolled Treasure Type: {CurrentType}");
        return CurrentType;
    }

    private TreasureType RollTreasureType()
    {
        float totalWeight = 0f;
        foreach (var entry in treasureTypeWeights)
        {
            if (entry != null && entry.weight > 0f)
                totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return default;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var entry in treasureTypeWeights)
        {
            if (entry == null || entry.weight <= 0f)
                continue;

            cumulative += entry.weight;
            if (roll < cumulative)
                return entry.type;
        }

        for (int i = treasureTypeWeights.Count - 1; i >= 0; i--)
        {
            var entry = treasureTypeWeights[i];
            if (entry != null && entry.weight > 0f)
                return entry.type;
        }

        return default;
    }
}
