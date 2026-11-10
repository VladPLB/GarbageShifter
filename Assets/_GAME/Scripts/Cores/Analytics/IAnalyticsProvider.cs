using System.Collections.Generic;

namespace _GAME.Scripts.Analytics
{
    public interface IAnalyticsProvider
    {
        void Initialize();
        void LogEvent(string name, IReadOnlyDictionary<string, object> parameters);
        void SetUserProperty(string name, string value);
    }
}
