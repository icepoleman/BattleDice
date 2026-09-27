using System.Collections.Generic;
using UnityEngine;

public class RewardRateConfig
{
    private static readonly Dictionary<string, List<TreasureBoxReward>> EnemyRewardDict = new Dictionary<string, List<TreasureBoxReward>>
    {
        ["Zako"] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 10, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 10, probability = 0.15f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 20, probability = 0.05f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 1, probability = 0.6f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 2, probability = 0.1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 3, probability = 0.05f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 0.05f },
        },
        ["Erito"] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 20, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 20, probability = 0.15f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 40, probability = 0.05f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 1, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 2, probability = 0.3f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 3, probability = 0.1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 5, probability = 0.05f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 0.1f },
        },
        ["Boss"] = new List<TreasureBoxReward>
        {
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 50, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 30, probability = 0.2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 20, probability = 0.05f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 1, probability = 1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 2, probability = 0.8f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 3, probability = 0.2f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gear, rewardValue = 5, probability = 0.1f },
            new TreasureBoxReward { rewardType = TreasureBoxRewardType.Skill, rewardValue = 1, probability = 0.2f },
        }
    };

    public static List<TreasureBoxReward> GetEnemyReward(string enemyType)
    {
        if (string.IsNullOrWhiteSpace(enemyType))
        {
            enemyType = "Zako";
        }

        if (!EnemyRewardDict.TryGetValue(enemyType, out var rewards) || rewards == null || rewards.Count == 0)
        {
            return new List<TreasureBoxReward>
            {
                new TreasureBoxReward { rewardType = TreasureBoxRewardType.Gold, rewardValue = 10, probability = 1f }
            };
        }

        return RollRewards(rewards);
    }

    public static List<TreasureBoxReward> GetEnemyRewards(string enemyType)
    {
        return GetEnemyReward(enemyType);
    }

    private static List<TreasureBoxReward> RollRewards(List<TreasureBoxReward> rewards)
    {
        List<TreasureBoxReward> result = new List<TreasureBoxReward>();
        if (rewards == null || rewards.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            float chance = Mathf.Max(0f, rewards[i].probability);
            if (chance <= 0f)
                continue;

            if (Random.value < chance)
            {
                bool merged = false;
                for (int j = 0; j < result.Count; j++)
                {
                    if (result[j].rewardType == rewards[i].rewardType)
                    {
                        result[j].rewardValue += rewards[i].rewardValue;
                        result[j].probability += rewards[i].probability;
                        merged = true;
                        break;
                    }
                }

                if (!merged)
                {
                    result.Add(new TreasureBoxReward
                    {
                        rewardType = rewards[i].rewardType,
                        rewardValue = rewards[i].rewardValue,
                        probability = rewards[i].probability
                    });
                }
            }
        }

        return result;
    }
}
