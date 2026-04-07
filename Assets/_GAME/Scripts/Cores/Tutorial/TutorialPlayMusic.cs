using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialPlayMusic : TutorialStepBase
    {
        [SerializeField] private MusicTrack _type;
        [SerializeField] private bool _fade = true;
        
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new MusicPlayEvent(_type, _fade), EventBus.EventRegion.GLOBAL);
        }
    }
}