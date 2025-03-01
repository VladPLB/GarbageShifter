using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Communications;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialStepAim : TutorialStepBase
    {
        public override bool IsComplete => true;
        
        public override void Play()
        {
            EventBus.Push( new ShowAimTutorialEvent(), EventBus.EventRegion.GAMEPLAY);
        }
    }
}