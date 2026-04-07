using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialPlayAmbient : TutorialStepBase
    {
        [SerializeField] private AmbientType _type;
        [SerializeField] private Transform _anchor;
        
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new AmbientPlayEvent(_type, _anchor), EventBus.EventRegion.GLOBAL);
        }
    }
}