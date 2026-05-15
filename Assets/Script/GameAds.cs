using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GameAds : MonoBehaviour
{
    public GeneralGameSettings m_gamesettings;
    public static GameAds instance;

    private string banner_id, rewarded_interstitial_id, rewarded_id;

    private BannerView banner_view;
    private RewardedInterstitialAd rewardedInterstitialAd;
    private RewardedAd rewardedAd;

    bool _loggedRewardedNotReady;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void initializeAdmobSdk()
    {
        if (m_gamesettings == null)
        {
            Debug.LogError("[GameAds] m_gamesettings is not assigned.");
            return;
        }

        PlayerPrefs.SetInt("isPlayer_BuyNoAds", 0);
        PlayerPrefs.Save();

        ApplyResolvedAdUnitIds();

        if (string.IsNullOrEmpty(banner_id) || string.IsNullOrEmpty(rewarded_id))
        {
            Debug.LogError("[GameAds] Missing ad unit id(s). Check General Game Settings.");
            return;
        }

        Debug.Log("[GameAds] Units — banner=" + banner_id + " | rewarded=" + rewarded_id +
                  " | test_Mode=" + m_gamesettings.test_Mode);

        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        AdMobRuntime.RequestInitializeOnce();

        if (AdMobRuntime.InitComplete)
            StartCoroutine(CoLoadAdsDeferred());
        else
            StartCoroutine(CoWaitForSdkThenLoadAds());
    }

    IEnumerator CoWaitForSdkThenLoadAds()
    {
        float waited;
        for (waited = 0f; !AdMobRuntime.InitComplete && waited < 15f; waited += Time.unscaledDeltaTime)
            yield return null;

        if (!AdMobRuntime.InitComplete)
            Debug.LogWarning("[GameAds] SDK init callback not received after 15s — attempting to load ads anyway.");

        yield return CoLoadAdsDeferred();
    }

    IEnumerator CoLoadAdsDeferred()
    {
        yield return null;
        if (this == null || instance != this)
            yield break;

        Debug.Log("[GameAds] Loading rewarded + banner.");
        requestToload_RewardedAd();

        if (check_RemoveAds_Status_toShow())
            RequestBanner();
    }

    void ApplyResolvedAdUnitIds()
    {
        banner_id = m_gamesettings.BannerAd_ID;
        rewarded_interstitial_id = m_gamesettings.rewardedInterstitialAdId;
        rewarded_id = m_gamesettings.rewardAdId;

        if (!m_gamesettings.test_Mode)
            return;

#if UNITY_ANDROID
        banner_id                = "ca-app-pub-3940256099942544/6300978111";
        rewarded_interstitial_id = "ca-app-pub-3940256099942544/5354046379";
        rewarded_id              = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
        banner_id                = "ca-app-pub-3940256099942544/2934735716";
        rewarded_interstitial_id = "ca-app-pub-3940256099942544/6978759866";
        rewarded_id              = "ca-app-pub-3940256099942544/1712485313";
#endif
        Debug.Log("[GameAds] test_Mode: Google sample IDs.");
    }


    private void Start()
    {
        initializeAdmobSdk();
    }


    bool check_RemoveAds_Status_toShow()
    {
        return true;
    }


    AdRequest AdRequestBuild()
    {
        return new AdRequest();
    }


    #region  banner ad

    bool is_banner_showed = false;
    public void showbannerAD()
    {
        if (check_RemoveAds_Status_toShow() && banner_view != null)
        {
            banner_view.Show();
            is_banner_showed = true;
        }
    }

    public bool isBannerShowing()
    {
        return is_banner_showed;
    }

    private void RequestBanner()
    {
        if (banner_view != null)
        {
            banner_view.Destroy();
            banner_view = null;
        }

        AdRequest request = AdRequestBuild();
        // Fixed 320x50 + Google's fixed-size test unit (see AdMob Unity test-ads doc).
        banner_view = new BannerView(banner_id, AdSize.Banner, AdPosition.Bottom);
        banner_view.OnBannerAdLoaded += OnBannerLoaded;
        banner_view.OnBannerAdLoadFailed += OnBannerLoadFailed;
        banner_view.LoadAd(request);
    }

    void OnBannerLoaded()
    {
        if (banner_view == null)
            return;
        Debug.Log("[GameAds] Banner loaded.");
        showbannerAD();
    }

    void OnBannerLoadFailed(LoadAdError error)
    {
        Debug.LogWarning("[GameAds] Banner load failed: " + (error != null ? error.ToString() : "(null)"));
    }

    #endregion


    public void ShowInterstitialAd()
    {
    }


    #region   reward_ads

    void requestToload_RewardedAd()
    {
        RewardedInterstitialAd.Load(rewarded_interstitial_id, AdRequestBuild(),
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[GameAds] Rewarded interstitial unavailable, loading fallback rewarded.");
                    LoadFallbackRewardedAd();
                    return;
                }
                rewardedInterstitialAd = ad;
                rewardedInterstitialAd.OnAdFullScreenContentClosed += () => StartCoroutine(reload_reward());
                rewardedInterstitialAd.OnAdFullScreenContentFailed += (_) => StartCoroutine(reload_reward());
                Debug.Log("[GameAds] Rewarded interstitial loaded.");
            });
    }

    void LoadFallbackRewardedAd()
    {
        RewardedAd.Load(rewarded_id, AdRequestBuild(),
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("[GameAds] Fallback rewarded load failed: " +
                                   (error != null ? error.ToString() : "ad null"));
                    var host = instance;
                    if (host != null)
                        host.StartCoroutine(host.reload_reward());
                    return;
                }
                rewardedAd = ad;
                rewardedAd.OnAdFullScreenContentClosed += () => StartCoroutine(reload_reward());
                rewardedAd.OnAdFullScreenContentFailed += (_) => StartCoroutine(reload_reward());
                Debug.Log("[GameAds] Fallback rewarded loaded.");
            });
    }

    public void showreward_Ad()
    {
        if (!check_RemoveAds_Status_toShow())
            return;

        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
            rewardedInterstitialAd.Show((Reward reward) => GrantRewardExtraBottle());
        else if (rewardedAd != null && rewardedAd.CanShowAd())
            rewardedAd.Show((Reward reward) => GrantRewardExtraBottle());
        else
            LogRewardedNotReadyOnce();
    }

    public void showreward_Ad_Skip()
    {
        if (!check_RemoveAds_Status_toShow())
            return;

        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
            rewardedInterstitialAd.Show((Reward reward) => GrantRewardSkipLevel());
        else if (rewardedAd != null && rewardedAd.CanShowAd())
            rewardedAd.Show((Reward reward) => GrantRewardSkipLevel());
        else
            LogRewardedNotReadyOnce();
    }

    void LogRewardedNotReadyOnce()
    {
        if (_loggedRewardedNotReady)
            return;
        _loggedRewardedNotReady = true;
        Debug.LogWarning("[GameAds] Rewarded not ready yet.");
    }

    IEnumerator reload_reward()
    {
        yield return new WaitForSeconds(2.0f);
        rewardedInterstitialAd = null;
        rewardedAd = null;
        requestToload_RewardedAd();
    }


    void GrantRewardExtraBottle()
    {
        if (GameScManger.instance != null)
            GameScManger.instance.reward_player_addBottle();
    }

    void GrantRewardSkipLevel()
    {
        if (GameScManger.instance != null)
            GameScManger.instance.reward_player_skip_level();
    }

    #endregion
}
