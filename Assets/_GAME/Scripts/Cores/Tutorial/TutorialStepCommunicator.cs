using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepCommunicator : TutorialStepBase
    {
        [SerializeField] private List<string> _messages = new();

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        
        public override void Play()
        {
            _isComplete = false;
            EventBus.Subscribe<DialogCompleteEvent>(OnComplete, EventBus.EventRegion.GAMEPLAY);
            EventBus.Push( new CommunicatorMessageEvent(_messages), EventBus.EventRegion.GAMEPLAY);
        }

        private void OnComplete(DialogCompleteEvent completeEvent)
        {
            EventBus.Unsubscribe<DialogCompleteEvent>(OnComplete, EventBus.EventRegion.GAMEPLAY);
            _isComplete = true;
        }
    }
}