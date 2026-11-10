using System.Collections.Generic;
using _GAME.Scripts.Ads;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Map;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Analytics
{
    // Sends AnalyticsEvent from the bus to the provider; every event gets base progress params
    public class AnalyticsManager : MonoBehaviour, IRuntimeSetup
    {
        [SerializeField] private bool _logToConsole = true;

        private IAnalyticsProvider _provider;
        private ProgressData _progressData;
        private InventoryManager _inventoryManager;
        private readonly Dictionary<string, string> _userProperties = new();

        private void Awake()
        {
            Core.Registry(this, typeof(SaveManager), typeof(InventoryManager));
        }

        public void RuntimeSetup()
        {
            _progressData = Core.Get<SaveManager>().GetData<ProgressData>();
            _inventoryManager = Core.Get<InventoryManager>();
#if FIREBASE_ANALYTICS && !UNITY_EDITOR
            _provider = new FirebaseAnalyticsProvider();
#else
            _provider = new DebugAnalyticsProvider(_logToConsole);
#endif
            _provider.Initialize();
            EventBus.Subscribe<AnalyticsEvent>(OnAnalyticsEvent, EventBus.EventRegion.GLOBAL);
        }

        private void OnAnalyticsEvent(AnalyticsEvent analyticsEvent)
        {
            var parameters = new Dictionary<string, object>(analyticsEvent.Params);
            AddBaseParams(parameters);
            UpdateUserProperties(parameters);
            _provider.LogEvent(analyticsEvent.Name, parameters);
        }

        private void AddBaseParams(Dictionary<string, object> parameters)
        {
            parameters["progress_level"] = _progressData.Level;
            parameters["location"] = GetLocation();
            parameters["no_ads"] = NoAds.IsActive(_inventoryManager);
            parameters["enemy_pressure"] = Mathf.Round((1f + _progressData.EnemyPressureOffset) * 100f) / 100f;
        }

        private int GetLocation()
        {
            var mapManager = Core.Get<MapManager>();
            return mapManager != null ? mapManager.GetLocationNumber(_progressData.Level) : -1;
        }

        // User properties are sent only when changed
        private void UpdateUserProperties(Dictionary<string, object> parameters)
        {
            foreach (var key in new[] { "progress_level", "location", "no_ads" })
            {
                var value = parameters[key] is bool b ? (b ? "1" : "0") : parameters[key].ToString();
                if (_userProperties.TryGetValue(key, out var previous) && previous == value)
                    continue;

                _userProperties[key] = value;
                _provider.SetUserProperty(key, value);
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AnalyticsEvent>(OnAnalyticsEvent, EventBus.EventRegion.GLOBAL);
            Core.Unregistry(this);
        }
    }
}
