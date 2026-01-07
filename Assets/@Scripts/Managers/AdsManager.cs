using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsManager : Singleton<AdsManager>
{
    LevelPlayInterstitialAd _interstitialAd;
    LevelPlayRewardedAd _rewardedAd;
    Action _rewardedCallback;

    #region 보상 처리
    public void ShowInterstitialAds()
    {
        _interstitialAd.LoadAd();
    }

    private void ShowInterstitialAds_AfterLoading()
    {
        if (_interstitialAd.IsAdReady())
            _interstitialAd.ShowAd();
    }

    public void ShowRewardedAds(Action rewardedCallback)
    {
        _rewardedCallback = rewardedCallback;
        _rewardedAd.LoadAd();
    }

    private void ShowRewardedAds_AfterLoading()
    {
        if (_rewardedAd.IsAdReady())
            _rewardedAd.ShowAd();
    }

    private void HandleRewards_AfterRewardedAd()
    {
        _rewardedCallback?.Invoke();
        _rewardedCallback = null;
    }
    #endregion

    public void Init()
    {
        Debug.Log("[LevelPlaySample] LevelPlay.ValidateIntegration");
        LevelPlay.ValidateIntegration();

        Debug.Log($"[LevelPlaySample] Unity version {LevelPlay.UnityVersion}");

        Debug.Log("[LevelPlaySample] Register initialization callbacks");
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        // SDK init
        Debug.Log("[LevelPlaySample] LevelPlay SDK initialization");
        LevelPlay.Init(DataManager.Instance.AdsConfig.GetAppKey());
    }

    void EnableAds()
    {
        // Register to ImpressionDataReadyEvent
        LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

        // Create Rewarded Video object
        _rewardedAd = new LevelPlayRewardedAd(DataManager.Instance.AdsConfig.GetRewardedVideoAdUnitId());

        // Register to Rewarded Video events
        _rewardedAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        _rewardedAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        _rewardedAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        _rewardedAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        _rewardedAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        _rewardedAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        _rewardedAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
        _rewardedAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;

        // Create Interstitial object
        _interstitialAd = new LevelPlayInterstitialAd(DataManager.Instance.AdsConfig.GetInterstitialAdUnitId());

        // Register to Interstitial events
        _interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        _interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        _interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        _interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        _interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
        _interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
        _interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
    }

    #region 로그
    void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationCompletedEvent with Config: {config}");
        EnableAds();
    }

    void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationFailedEvent with Error: {error}");
    }

    void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
        ShowRewardedAds_AfterLoading();
    }

    void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
    }

    void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
        HandleRewards_AfterRewardedAd();
    }

    void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
        ShowInterstitialAds_AfterLoading();
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
    }

    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
    }

    void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
    {
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
    }
    #endregion

}