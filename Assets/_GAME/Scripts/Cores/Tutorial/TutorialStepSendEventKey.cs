using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepSendEventKey : TutorialStepBase
    {
        [SerializeField] private string _key;
        [SerializeField] private EventBus.EventRegion _region = EventBus.EventRegion.GAMEPLAY;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new KeyEvent(_key), _region);
        }
    }
}