using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static bool Initialized { get; private set; }

    private void Awake()
    {
        if (Initialized)
        {
            Destroy(gameObject);
            return;
        }

        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI.Init() failed!");
            return;
        }

        Initialized = true;
        DontDestroyOnLoad(gameObject);

        Debug.Log("Steamworks initialized!");
    }

    private void Update()
    {
        if (Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
            Initialized = false;
        }
    }
}