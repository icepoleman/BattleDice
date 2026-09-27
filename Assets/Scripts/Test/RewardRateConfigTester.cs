using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardRateConfigTester : MonoBehaviour
{
    [Header("測試按鈕")]
    [SerializeField] private Button btn_zako;
    [SerializeField] private Button btn_erto;
    [SerializeField] private Button btn_boss;

    private void Start()
    {
        if (btn_zako != null)
            btn_zako.onClick.AddListener(() => LogRewards("Zako"));

        if (btn_erto != null)
            btn_erto.onClick.AddListener(() => LogRewards("Erito"));

        if (btn_boss != null)
            btn_boss.onClick.AddListener(() => LogRewards("Boss"));
    }

    private void LogRewards(string enemyType)
    {
        List<TreasureBoxReward> rewards = RewardRateConfig.GetEnemyReward(enemyType);

        if (rewards == null || rewards.Count == 0)
        {
            Debug.Log($"[{enemyType}] 沒有抽到獎勵");
            return;
        }

        string logText = $"[{enemyType}] 抽中獎勵：";
        for (int i = 0; i < rewards.Count; i++)
        {
            var reward = rewards[i];
            logText += $"{reward.rewardType} x{reward.rewardValue} (prob={reward.probability})";
            if (i < rewards.Count - 1)
                logText += " | ";
        }

        Debug.Log(logText);
    }
}
