using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepDialog : TutorialStepBase
    {
        [SerializeField] private NPCName _npc;
        [SerializeField] private List<string> _messages = new();
        [SerializeField] private List<NPCAnimationType> _animations = new();

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        
        public override void Play()
        {
            _isComplete = false;
            EventBus.Subscribe<DialogCompleteEvent>(OnComplete, EventBus.EventRegion.LOBBY);
            EventBus.Push( new DialogMessageEvent(_npc,_messages, _animations), EventBus.EventRegion.LOBBY);
        }

        private void OnComplete(DialogCompleteEvent completeEvent)
        {
            EventBus.Unsubscribe<DialogCompleteEvent>(OnComplete, EventBus.EventRegion.LOBBY);
            _isComplete = true;
        }
    }
}