using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepShowFadeEvent : TutorialStepBase
    {
        [SerializeField] private bool _isShow;
        [SerializeField] private float _duration;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new ShowBlackFadeEvent(_isShow, _duration), EventBus.EventRegion.GLOBAL);
        }
    }
}