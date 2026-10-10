using Steamworks;
using UnityEngine;
public class SteamAchievementManager
{
    private static readonly SteamAchievementManager instance = new SteamAchievementManager();

    public static SteamAchievementManager Instance
    {
        get { return instance; }
    }
    private SteamAchievementManager()
    {
    }

    public void UnlockAchievement(string achievementId)
    {
        Debug.Log($"UnlockAchievement: {achievementId}");
        SteamUserStats.SetAchievement(achievementId);
        SteamUserStats.StoreStats();
        /*Debug.Log("Steam Initialized: " + SteamManager.Initialized);

        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Cannot query achievements before Steam is initialized.");
            return;
        }

        uint numAchievements = SteamUserStats.GetNumAchievements();
        Debug.Log("Achievement Count: " + numAchievements);

        bool exists = SteamUserStats.GetAchievement(
            "TEST_001",
            out bool achieved
        );

        Debug.Log("TEST_001 Exists: " + exists);
        Debug.Log("TEST_001 Achieved: " + achieved);*/
    }
}
