using System;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class SoundEventTrigger: MonoBehaviour
    {
        [SerializeField]
        private SoundType _sound;
        [SerializeField]
        private Transform _point;

        public void Sound()
        {
            EventBus.Push(new SoundPlayEvent(_sound, _point!=null? _point.position: null), EventBus.EventRegion.GLOBAL);
        }
    }
}