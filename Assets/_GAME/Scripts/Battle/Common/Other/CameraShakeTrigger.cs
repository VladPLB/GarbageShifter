using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class CameraShakeTrigger: MonoBehaviour
    {
        [SerializeField, Range(0f,1f)]
        private float _shakeForce = 1f;
        [SerializeField, Range(0f,1f)]
        private float _glitchForce = .5f;
        [SerializeField, Range(0f,1f)]
        private float _glitchDuration = .2f;
        
        public void Play()
        {
            EventBus.Push(new CameraShakeEvent(_shakeForce, _glitchForce,_glitchDuration), EventBus.EventRegion.GAMEPLAY);
        }
    }
}