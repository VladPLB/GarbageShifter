using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepLobbyToPlaceEvent : TutorialStepBase
    {
        [SerializeField] private LobbyPlaceType _type;
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push(new ToLobbyPlaceEvent(_type), EventBus.EventRegion.LOBBY);
        }
    }
}