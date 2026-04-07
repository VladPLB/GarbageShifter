using System.Collections.Generic;
using _GAME.Scripts.Audio;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class SoundPlayEvent: IEvent
    {
        public SoundType SoundType;
        public Vector3? Position;

        public SoundPlayEvent(SoundType type, Vector3? position) => (SoundType, Position) = (type, position);
    }
}