using System;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class AmbientEventTrigger: MonoBehaviour
    {
        [SerializeField]
        private AmbientType _ambientType;
        [SerializeField]
        private Transform _point;

        public void AmbientPlay()
        {
            AudioManager.Play(_ambientType, _point);
        }
        
        public void AmbientStop()
        {
            AudioManager.Stop(_ambientType);
        }
    }
}