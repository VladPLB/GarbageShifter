using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepMaxSeccondaryValueEvent : TutorialStepBase
    {
        [SerializeField, Range(0f,1f)] private float _maxValue;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new SeccondaryMaxValueEvent(_maxValue), EventBus.EventRegion.GAMEPLAY);
        }
    }
}