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
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new KeyEvent(_key), EventBus.EventRegion.GAMEPLAY);
        }
    }
}