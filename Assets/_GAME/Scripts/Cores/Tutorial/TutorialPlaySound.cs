using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialPlaySound : TutorialStepBase
    {
        [SerializeField] private SoundType _type;
        [SerializeField] private Vector3 _position;
        
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new SoundPlayEvent(_type, _position== Vector3.zero?null:_position), EventBus.EventRegion.GLOBAL);
        }
    }
}