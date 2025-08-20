using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepAwaitLobbyPlaceEventKey : TutorialStepBase
    {
        [SerializeField] private LobbyPlaceType _type;

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        public override void Play()
        {
            _isComplete = false;
            EventBus.Subscribe<OnLobbyPlaceEvent>(OnEvent, EventBus.EventRegion.LOBBY);
        }
        
        public void OnEvent(OnLobbyPlaceEvent keyEvent)
        {
            if(keyEvent.Type == _type)
            {
                EventBus.Unsubscribe<OnLobbyPlaceEvent>(OnEvent, EventBus.EventRegion.LOBBY);
                _isComplete = true;
            }
        }
    }
}