using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepShowLobbyPanelEvent : TutorialStepBase
    {
        [SerializeField] private string _panelName;
        [SerializeField] private bool _isShow;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new SetEnableLobbyPanelEvent(_panelName, _isShow), EventBus.EventRegion.LOBBY);
        }
    }
}