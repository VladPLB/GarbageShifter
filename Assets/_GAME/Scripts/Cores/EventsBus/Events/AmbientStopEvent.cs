using System.Collections.Generic;
using _GAME.Scripts.Audio;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class AmbientStopEvent: IEvent
    {
        public AmbientType AmbientType;
        public bool Fade;

        public AmbientStopEvent(AmbientType type, bool fade = true) => (AmbientType, Fade) = (type, fade);
    }
}