using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class CameraShakeTrigger: MonoBehaviour
    {
        [SerializeField, Range(0f,1f)]
        private float _shakeForce = 1f;
        [SerializeField, Range(0f, 5f)] 
        private float _darkDuration = 0f;
        [SerializeField, Range(0f,1f)]
        private float _glitchForce = .5f;
        [SerializeField, Range(0f,2f)]
        private float _glitchDuration = .5f;
        
        public void Play()
        {
            EventBus.Push(new CameraShakeEvent(_shakeForce, _darkDuration, _glitchForce,_glitchDuration), EventBus.EventRegion.GAMEPLAY);
        }
    }
}