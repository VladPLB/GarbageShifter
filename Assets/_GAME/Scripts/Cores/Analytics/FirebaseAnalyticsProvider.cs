#if FIREBASE_ANALYTICS
using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;

namespace _GAME.Scripts.Analytics
{
    // Requires Firebase Analytics SDK + google-services.json and the FIREBASE_ANALYTICS scripting define
    public class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        private bool _isReady;
        private readonly List<Action> _pending = new();

        public void Initialize()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError($"[Analytics] Firebase dependencies are not available: {task.Result}");
                    return;
                }

                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                _isReady = true;
                foreach (var action in _pending)
                    action();
                _pending.Clear();
            });
        }

        public void LogEvent(string name, IReadOnlyDictionary<string, object> parameters)
        {
            var firebaseParams = parameters.Select(p => ToParameter(p.Key, p.Value)).ToArray();
            Run(() => FirebaseAnalytics.LogEvent(name, firebaseParams));
        }

        public void SetUserProperty(string name, string value)
        {
            Run(() => FirebaseAnalytics.SetUserProperty(name, value));
        }

        private void Run(Action action)
        {
            if (_isReady)
                action();
            else
                _pending.Add(action);
        }

        private static Parameter ToParameter(string key, object value)
        {
            return value switch
            {
                bool b => new Parameter(key, b ? 1L : 0L),
                int i => new Parameter(key, (long)i),
                long l => new Parameter(key, l),
                float f => new Parameter(key, (double)f),
                double d => new Parameter(key, d),
                _ => new Parameter(key, value?.ToString() ?? string.Empty),
            };
        }
    }
}
#endif
