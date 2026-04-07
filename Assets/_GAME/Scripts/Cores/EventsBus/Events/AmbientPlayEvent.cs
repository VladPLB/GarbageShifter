using System.Collections.Generic;
using _GAME.Scripts.Audio;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class AmbientPlayEvent: IEvent
    {
        public AmbientType AmbientType;
        public Transform Anchor;

        public AmbientPlayEvent(AmbientType type, Transform anchor = null) => (AmbientType, Anchor) = (type, anchor);
    }
}