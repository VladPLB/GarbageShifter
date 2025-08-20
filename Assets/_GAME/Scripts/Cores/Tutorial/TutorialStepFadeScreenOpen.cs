using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepFadeScreenOpen : TutorialStepBase
    {
        [SerializeField] private int _id;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new OpenTutorFadeScreenEvent(_id), EventBus.EventRegion.GLOBAL);
        }
    }
}