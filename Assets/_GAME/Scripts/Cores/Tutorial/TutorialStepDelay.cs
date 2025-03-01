using System;
using System.Collections.Generic;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepDelay : TutorialStepBase
    {
        [SerializeField] private float _delay;

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        
        public override async void Play()
        {
            _isComplete = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_delay));
            _isComplete = true;
        }
    }
}