using System.Collections.Generic;
using _GAME.Scripts.Audio;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class MusicPlayEvent: IEvent
    {
        public MusicTrack MusicTrack;
        public bool Fade = true;

        public MusicPlayEvent(MusicTrack type) => MusicTrack = type;
        public MusicPlayEvent(MusicTrack type, bool fade) => (MusicTrack, Fade) = (type, fade);
    }
}