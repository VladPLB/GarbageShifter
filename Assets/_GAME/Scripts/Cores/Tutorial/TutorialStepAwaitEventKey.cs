using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepAwaitEventKey : TutorialStepBase
    {
        [SerializeField] private string _key;

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        public override void Play()
        {
            _isComplete = false;
            EventBus.Subscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GAMEPLAY);
        }
        
        public void OnEvent(KeyEvent keyEvent)
        {
            if(keyEvent.Key == _key)
            {
                EventBus.Unsubscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GAMEPLAY);
                _isComplete = true;
            }
        }
    }
}