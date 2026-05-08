using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GameAds : MonoBehaviour
{
    public GeneralGameSettings m_gamesettings;
    public static GameAds instance;

    private string game_id, banner_id, rewarded_id;
    private bool testMode;


    private BannerView banner_view;
    private RewardedAd rewardedAd;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        else
        {
            instance = this;
        }
    }

    public void initializeAdmobSdk()
    {

        game_id = m_gamesettings.gameId;
        banner_id = m_gamesettings.BannerAd_ID;
        rewarded_id = m_gamesettings.rewardAdId;

        int remove_ads_status = PlayerPrefs.GetInt("isPlayer_BuyNoAds", 0);

        MobileAds.Initialize(initStatus => { });

        MobileAds.SetApplicationMuted(true);
        MobileAds.SetApplicationVolume(0);

        requestToload_RewardedAd();

        if (check_RemoveAds_Status_toShow() == true)  // if its true then you are able to show ads, the player doesnt buy the item yet
        {

            //Debug.Log("show ads no remove ads"); 

            RequestBanner();
        }

    }



    private void Start()
    {
        initializeAdmobSdk();
    }


    bool check_RemoveAds_Status_toShow()
    {
        int remove_ads_status = PlayerPrefs.GetInt("isPlayer_BuyNoAds", 0);

        if (remove_ads_status == 0)
        {
            //show ads 
            return true;
        }
        else
        {
            return false;
        }

    }


    AdRequest AdRequestBuild()
    {
        AdRequest request = new AdRequest();
        return request;
    }


    //*************************************************************** baner ad
    #region  banner ad

    bool is_banner_showed = false;
    public void showbannerAD()
    {

        if (check_RemoveAds_Status_toShow() == true)
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
        // Create an empty ad request.
        AdRequest request = AdRequestBuild();
        // Create a 320x50 banner at the top of the screen.
        banner_view = new BannerView(banner_id, AdSize.Banner, AdPosition.Bottom);
        // Load the banner with the request.
        banner_view.LoadAd(request);

       // showbannerAD();
    }

    #endregion


    //*************************************************************** interstitial ad (disabled — monetization uses rewarded for skip / extra bottle)
    #region  interstitial Ad

    public void ShowInterstitialAd()
    {
        // Intentionally empty: interstitials removed in favor of rewarded ads on skip & add-bottle.
    }

    #endregion



    #region   reward_ads

    void requestToload_RewardedAd()
    {
        AdRequest request = AdRequestBuild();

        RewardedAd.Load(rewarded_id, request,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        StartCoroutine(reload_reward());
                        return;
                    }
                    rewardedAd = ad;
                    RegisterEventHandlers(rewardedAd);
                });
    }

    /// <summary>Watch ad to add an extra bottle (when allowed).</summary>
    public void showreward_Ad()
    {
        if (!check_RemoveAds_Status_toShow())
            return;

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) => GrantRewardExtraBottle());
        }
    }


    /// <summary>Watch ad to skip the current level.</summary>
    public void showreward_Ad_Skip()
    {
        if (!check_RemoveAds_Status_toShow())
            return;

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) => GrantRewardSkipLevel());
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            StartCoroutine(reload_reward());
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            StartCoroutine(reload_reward());
        };
    }



    IEnumerator reload_reward()
    {
        yield return new WaitForSeconds(2.0f);
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
