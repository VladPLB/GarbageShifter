using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Analytics
{
    // Editor / no SDK: events are only written to the console
    public class DebugAnalyticsProvider : IAnalyticsProvider
    {
        private readonly bool _logToConsole;

        public DebugAnalyticsProvider(bool logToConsole) => _logToConsole = logToConsole;

        public void Initialize()
        {
        }

        public void LogEvent(string name, IReadOnlyDictionary<string, object> parameters)
        {
            if (_logToConsole)
                Debug.Log($"[Analytics] {name}: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
        }

        public void SetUserProperty(string name, string value)
        {
            if (_logToConsole)
                Debug.Log($"[Analytics] user property {name}={value}");
        }
    }
}
