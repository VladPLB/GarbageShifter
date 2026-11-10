using System;
using System.Collections.Generic;
using _GAME.Scripts.Map;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Ads
{
    // Bridge between the game and an ads service; every show request is reported to analytics with its placement
    public class AdsManager : MonoBehaviour, IRuntimeSetup
    {
        [Serializable]
        private struct InterstitialFrequency
        {
            [Tooltip("Rule works while the location number (0-based) is below this value")]
            public int UntilLocation;
            [Min(1)] public int EveryNthGame;
        }

        [Header("Google Mobile Ads (Android), defaults are Google test units")]
        [SerializeField] private string _androidRewardedUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _androidInterstitialUnitId = "ca-app-pub-3940256099942544/1033173712";

        [Header("Interstitial")]
        [Tooltip("Min seconds between interstitials (also counted after a rewarded ad)")]
        [SerializeField, Min(0f)] private float _interstitialCooldown = 90f;
        [Tooltip("No interstitials before this progress level")]
        [SerializeField, Min(0)] private int _interstitialMinLevel = 5;
        [Tooltip("Show on every N-th played level by location; first matching rule wins, otherwise every game")]
        [SerializeField] private List<InterstitialFrequency> _interstitialFrequency = new()
        {
            new InterstitialFrequency { UntilLocation = 10, EveryNthGame = 3 },
            new InterstitialFrequency { UntilLocation = 20, EveryNthGame = 2 },
        };

        [Header("Debug (editor / no SDK)")]
        [SerializeField] private AdShowResult _debugResult = AdShowResult.Completed;

        private IAdsProvider _provider;
        private InventoryManager _inventoryManager;
        private float _lastFullscreenAdTime = float.MinValue;
        private bool _isShowing;
        private string _pendingInterstitialPlacement;

        public bool IsNoAds => NoAds.IsActive(_inventoryManager);

        private void Awake()
        {
            Core.Registry(this, typeof(InventoryManager));
        }

        public void RuntimeSetup()
        {
            _inventoryManager = Core.Get<InventoryManager>();
#if GOOGLE_MOBILE_ADS && !UNITY_EDITOR
            _provider = new GoogleAdsProvider(_androidRewardedUnitId, _androidInterstitialUnitId);
#else
            _provider = new DebugAdsProvider(_debugResult);
#endif
            _provider.Initialize();
            EventBus.Subscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete, EventBus.EventRegion.GLOBAL);
        }

        // Interstitial after a level is shown when the lobby is loaded
        public void QueueInterstitial(string placement)
        {
            _pendingInterstitialPlacement = placement;
        }

        private void OnSceneLoadComplete(SceneLoadCompleteEvent loadEvent)
        {
            if (string.IsNullOrEmpty(_pendingInterstitialPlacement))
                return;

            var placement = _pendingInterstitialPlacement;
            _pendingInterstitialPlacement = null;
            TryShowInterstitial(placement, null);
        }

        public bool IsRewardedReady => _provider != null && _provider.IsReady(AdType.Rewarded);

        // Rewarded ads are shown even with NoAds; onComplete(true) - reward granted
        public void ShowRewarded(string placement, Action<bool> onComplete)
        {
            Show(AdType.Rewarded, placement, result => onComplete?.Invoke(result == AdShowResult.Completed));
        }

        // onDone is always called (shown, skipped by NoAds/cooldown or failed)
        public void TryShowInterstitial(string placement, Action onDone)
        {
            if (IsNoAds)
            {
                ReportAd(AdType.Interstitial, placement, "blocked_no_ads");
                onDone?.Invoke();
                return;
            }

            if (!IsInterstitialAllowed())
            {
                onDone?.Invoke();
                return;
            }

            Show(AdType.Interstitial, placement, _ => onDone?.Invoke());
        }

        // cooldown, min progress level and "every N-th game" by location
        private bool IsInterstitialAllowed()
        {
            if (Time.realtimeSinceStartup - _lastFullscreenAdTime < _interstitialCooldown)
                return false;

            var progress = Core.Get<SaveManager>()?.GetData<ProgressData>();
            if (progress == null || progress.Level < _interstitialMinLevel)
                return false;

            var mapManager = Core.Get<MapManager>();
            var location = mapManager != null ? mapManager.GetLocationNumber(progress.Level) : 0;
            var everyNth = 1;
            foreach (var rule in _interstitialFrequency)
            {
                if (location < rule.UntilLocation)
                {
                    everyNth = Mathf.Max(1, rule.EveryNthGame);
                    break;
                }
            }
            return progress.LevelsPlayed % everyNth == 0;
        }

        private void Show(AdType type, string placement, Action<AdShowResult> callback)
        {
            if (_provider == null || _isShowing || !_provider.IsReady(type))
            {
                ReportAd(type, placement, "not_ready");
                callback?.Invoke(AdShowResult.Failed);
                return;
            }

            _isShowing = true;
            ReportAd(type, placement, "show");
            _provider.Show(type, result =>
            {
                _isShowing = false;
                if (result != AdShowResult.Failed)
                    _lastFullscreenAdTime = Time.realtimeSinceStartup;

                ReportAd(type, placement, result.ToString().ToLowerInvariant());
                callback?.Invoke(result);
            });
        }

        private static void ReportAd(AdType type, string placement, string result)
        {
            AnalyticsEvent.Push("ad_event",
                ("ad_type", type.ToString().ToLowerInvariant()),
                ("placement", placement),
                ("result", result));
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete, EventBus.EventRegion.GLOBAL);
            Core.Unregistry(this);
        }
    }
}
