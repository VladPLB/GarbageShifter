using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepOpenDialogScreenEvent : TutorialStepBase
    {
        [SerializeField] private bool _isOpen;
        [SerializeField] private UIDialog.PositionType _positionType;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new OpenDialogScreenEvent(_isOpen, _positionType), EventBus.EventRegion.LOBBY);
        }
    }
}