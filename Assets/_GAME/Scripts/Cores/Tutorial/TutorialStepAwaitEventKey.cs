using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepAwaitEventKey : TutorialStepBase
    {
        [SerializeField] private string _key;
        [SerializeField] private EventBus.EventRegion _region = EventBus.EventRegion.GAMEPLAY;

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        public override void Play()
        {
            _isComplete = false;
            EventBus.Subscribe<KeyEvent>(OnEvent, _region);
        }
        
        public void OnEvent(KeyEvent keyEvent)
        {
            Debug.Log($"TutorialStepAwaitEventKey: {keyEvent.Key}");
            if(keyEvent.Key == _key)
            {
                EventBus.Unsubscribe<KeyEvent>(OnEvent, _region);
                _isComplete = true;
            }
        }
    }
}