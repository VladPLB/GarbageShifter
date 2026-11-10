#if GOOGLE_MOBILE_ADS
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace _GAME.Scripts.Ads
{
    // Requires Google Mobile Ads Unity plugin (+ app id in its settings) and the GOOGLE_MOBILE_ADS scripting define
    public class GoogleAdsProvider : IAdsProvider
    {
        private readonly string _rewardedUnitId;
        private readonly string _interstitialUnitId;

        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;

        public GoogleAdsProvider(string rewardedUnitId, string interstitialUnitId)
        {
            _rewardedUnitId = rewardedUnitId;
            _interstitialUnitId = interstitialUnitId;
        }

        public void Initialize()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(_ =>
            {
                LoadRewarded();
                LoadInterstitial();
            });
        }

        public bool IsReady(AdType type)
        {
            return type == AdType.Rewarded
                ? _rewardedAd != null && _rewardedAd.CanShowAd()
                : _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public void Show(AdType type, Action<AdShowResult> callback)
        {
            if (!IsReady(type))
            {
                callback?.Invoke(AdShowResult.Failed);
                return;
            }

            if (type == AdType.Rewarded)
                ShowRewarded(callback);
            else
                ShowInterstitial(callback);
        }

        private void ShowRewarded(Action<AdShowResult> callback)
        {
            var ad = _rewardedAd;
            _rewardedAd = null;
            var isRewarded = false;
            ad.OnAdFullScreenContentClosed += () =>
            {
                callback?.Invoke(isRewarded ? AdShowResult.Completed : AdShowResult.Skipped);
                ad.Destroy();
                LoadRewarded();
            };
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogWarning($"[Ads] Rewarded show failed: {error}");
                callback?.Invoke(AdShowResult.Failed);
                ad.Destroy();
                LoadRewarded();
            };
            ad.Show(_ => isRewarded = true);
        }

        private void ShowInterstitial(Action<AdShowResult> callback)
        {
            var ad = _interstitialAd;
            _interstitialAd = null;
            ad.OnAdFullScreenContentClosed += () =>
            {
                callback?.Invoke(AdShowResult.Completed);
                ad.Destroy();
                LoadInterstitial();
            };
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogWarning($"[Ads] Interstitial show failed: {error}");
                callback?.Invoke(AdShowResult.Failed);
                ad.Destroy();
                LoadInterstitial();
            };
            ad.Show();
        }

        private void LoadRewarded()
        {
            RewardedAd.Load(_rewardedUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Rewarded load failed: {error}");
                    return;
                }
                _rewardedAd = ad;
            });
        }

        private void LoadInterstitial()
        {
            InterstitialAd.Load(_interstitialUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Interstitial load failed: {error}");
                    return;
                }
                _interstitialAd = ad;
            });
        }
    }
}
#endif
