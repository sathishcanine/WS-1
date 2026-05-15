using UnityEngine;
using GoogleMobileAds.Api;

/// <summary>
/// Starts the Mobile Ads SDK as early as possible (main menu). Game scene only loads ads.
/// </summary>
public static class AdMobRuntime
{
    public static bool InitRequested { get; private set; }
    public static bool InitComplete { get; private set; }

    public static void RequestInitializeOnce()
    {
        if (InitRequested)
            return;

        InitRequested = true;
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        Debug.Log("[AdMobRuntime] MobileAds.Initialize() starting.");
        MobileAds.Initialize(status =>
        {
            InitComplete = true;
            Debug.Log("[AdMobRuntime] MobileAds.Initialize() finished.");
        });
    }
}
