using UnityEngine;

[CreateAssetMenu(fileName = "AdsConfig", menuName = "Config/AdsConfig")]
public class AdsConfig : ScriptableObject
{
    [Header("Android Settings")]
    [SerializeField]
    private string android_app_id;
    [SerializeField]
    private string android_interstitial_id;
    [SerializeField]
    private string android_rewarded_id;

    [Header("iOS Settings")]
    [SerializeField]
    private string ios_app_id;
    [SerializeField]
    private string ios_interstitial_id;
    [SerializeField]
    private string ios_rewarded_id;

    #region IDs
    public string GetAppKey()
    {
#if UNITY_ANDROID
        return android_app_id;
#elif UNITY_IPHONE
        return ios_app_id;
#else
        return "unexpected_platform";
#endif
    }

    public string GetInterstitialAdUnitId()
    {
#if UNITY_ANDROID
        return android_interstitial_id;
#elif UNITY_IPHONE
		return ios_interstitial_id;
#else
        return "unexpected_platform";
#endif
    }

    public string GetRewardedVideoAdUnitId()
    {
#if UNITY_ANDROID
        return android_rewarded_id;
#elif UNITY_IPHONE
		return ios_rewarded_id;
#else
        return "unexpected_platform";
#endif
    }
    #endregion
}

