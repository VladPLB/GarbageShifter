using System;
using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public static class EventBus
    {
        public enum EventRegion
        {
            DEFAULT,
            GLOBAL,
            GAMEPLAY,
            LOBBY
        }
        
        private static readonly Dictionary<EventRegion, Dictionary<Type, List<Delegate>>> _events = new();

        public static void Subscribe<T>(Action<T> listener, EventRegion region) where T:IEvent
        {
            region = region == EventRegion.DEFAULT ? EventRegion.GLOBAL : region;
            if (!_events.ContainsKey(region))
            {
                _events.Add(region, new Dictionary<Type, List<Delegate>>());
            }
            
            if (!_events[region].ContainsKey(typeof(T)))
            {
                _events[region].Add(typeof(T), new List<Delegate>());
            }
            _events[region][typeof(T)].Add(listener);
        }
        
        public static void Unsubscribe<T>(Action<T> listener, EventRegion region) where T:IEvent
        {
            region = region == EventRegion.DEFAULT ? EventRegion.GLOBAL : region;
            if(!_events.ContainsKey(region))
                return;
            
            if (_events[region].TryGetValue(typeof(T), out var delegates))
            {
                delegates.Remove(listener);
                if (delegates.Count == 0)
                {
                    _events[region].Remove(typeof(T));
                }
            }
        }
        
        public static void Push<T>(T eventData, EventRegion region) where T:IEvent
        {
            if (region == EventRegion.DEFAULT)
            {
                foreach (var key in _events.Keys)
                {
                    if (_events[key].TryGetValue(typeof(T), out var delegates))
                    {
                        foreach (var del in delegates)
                        {
                            (del as Action<T>)?.Invoke(eventData);
                        }
                    }
                }
            }
            else if(_events.ContainsKey(region))
            {
                if (_events[region].TryGetValue(typeof(T), out var delegates))
                {
                    foreach (var del in delegates)
                    {
                        (del as Action<T>)?.Invoke(eventData);
                    }
                }
            }
        }

        public static void Clear(EventRegion region = EventRegion.DEFAULT)
        {
            if(region == EventRegion.DEFAULT)
            {
                _events.Clear();
            }
            else
            {
                _events[region].Clear();
            }
        }
    }
}