using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    // Game code reports analytics through the bus; AnalyticsManager adds base progress params and sends it
    public class AnalyticsEvent: IEvent
    {
        public readonly string Name;
        public readonly Dictionary<string, object> Params;

        public AnalyticsEvent(string name, Dictionary<string, object> parameters)
        {
            Name = name;
            Params = parameters ?? new Dictionary<string, object>();
        }

        public static void Push(string name, params (string Key, object Value)[] parameters)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var (key, value) in parameters)
            {
                if (value != null)
                    dictionary[key] = value;
            }
            EventBus.Push(new AnalyticsEvent(name, dictionary), EventBus.EventRegion.GLOBAL);
        }
    }
}
