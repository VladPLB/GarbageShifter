using System;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class KeyEventTrigger: MonoBehaviour
    {
        [SerializeField]
        private string _key = String.Empty;
        [SerializeField]
        private EventBus.EventRegion _eventRegion = EventBus.EventRegion.GAMEPLAY;

        public void Send()
        {
            if (!string.IsNullOrEmpty(_key))
            {
                EventBus.Push(new KeyEvent(_key), _eventRegion);
            }
        }
    }
}