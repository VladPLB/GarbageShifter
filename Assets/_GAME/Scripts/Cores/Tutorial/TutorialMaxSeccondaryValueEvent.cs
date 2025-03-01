using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialMaxSeccondaryValueEvent : TutorialStepBase
    {
        [SerializeField, Range(0f,1f)] private float _maxValue;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new SeccondaryMaxValueEvent(_maxValue), EventBus.EventRegion.GAMEPLAY);
        }
    }
}