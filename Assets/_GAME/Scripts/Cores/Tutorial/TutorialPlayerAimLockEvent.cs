using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialPlayerAimLockEvent : TutorialStepBase
    {
        [SerializeField] private bool _lock;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new AimLockEvent(_lock), EventBus.EventRegion.GAMEPLAY);
        }
    }
}