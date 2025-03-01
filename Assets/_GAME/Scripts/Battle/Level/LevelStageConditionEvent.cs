using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStageConditionEvent : levelStageConditionBase
    {
        [SerializeField] private string _eventKey;
        private bool _isComplete = false;
        public override bool IsNext =>_isSkipStage ||  _isComplete;

        public override void Setup(LevelStage stage)
        {
            base.Setup(stage);
            EventBus.Subscribe<KeyEvent>(OnComplete, EventBus.EventRegion.GAMEPLAY);
        }

        public void OnComplete(KeyEvent keyEvent)
        {
            if(keyEvent.Key == _eventKey)
            {
                EventBus.Unsubscribe<KeyEvent>(OnComplete, EventBus.EventRegion.GAMEPLAY);
                _isComplete = true;
            }
        }
    }
}