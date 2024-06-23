using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GameAds : MonoBehaviour
{
    public GeneralGameSettings m_gamesettings;
    public static GameAds instance;

    private string game_id, interstitial_id, banner_id, rewarded_id;
    private bool testMode;


    private BannerView banner_view;
    private InterstitialAd interstitial;
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
        interstitial_id = m_gamesettings.InterstitialAdId;
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
            RequestInterstitial();
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


    //*************************************************************** interstitial ad
    #region  interstitial Ad


    public void ShowInterstitialAd()
    {
        if (check_RemoveAds_Status_toShow() == true && interstitial != null && interstitial.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitial.Show();
        }
    }

    private void RequestInterstitial()
    {
        AdRequest request = AdRequestBuild();

        // send the request to load the ad.
        InterstitialAd.Load(interstitial_id, request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    return;
                }


                interstitial = ad;
            });
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
                        return;
                    }
                    rewardedAd = ad;
                });
    }

    public void showreward_Ad()
    {
        if (rewardedAd != null && this.rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                getuserRewarded();

            });
        }
    }


  


    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // the ads field to load 

        StartCoroutine(reload_reward());

        //load after 2 seconds
    }


    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            reload_reward();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            reload_reward();
        };
    }



    IEnumerator reload_reward()
    {
        yield return new WaitForSeconds(2.0f);
        requestToload_RewardedAd();
    }


    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log("reward add field to show");
    }



    public void HandleUserEarnedReward(object sender, Reward args)
    {
        //reward the user
        getuserRewarded();

    }


    void getuserRewarded()
    {
        int status_reward = PlayerPrefs.GetInt("reward_stats", -1);

        if (status_reward == 1)
        {

            GameScManger.instance.reward_player_addBottle();

        }

        requestToload_RewardedAd();
    }

    #endregion


}
