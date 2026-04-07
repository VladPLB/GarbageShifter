using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class CameraController : MonoBehaviour
    {
        [Serializable]
        public struct VCameraByType
        {
            public GameCameraType Type;
            public CinemachineCamera VCamera;
        }
        
        [SerializeField] private List<VCameraByType> _cameras;
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        [SerializeField] private Light _directionLight;

        private float _defaultIntencity;

        public void SetCamera(GameCameraType type, Transform follow= null,  Transform target=null)
        {
            foreach (VCameraByType camera in _cameras)
            {
                if (camera.Type == type)
                {
                    if (follow != null)
                    {
                        camera.VCamera.Follow = follow;
                    }

                    if (target != null)
                    {
                        camera.VCamera.LookAt = target;
                    }
                }
                camera.VCamera.enabled = camera.Type == type;
                
            }
            _cameras.ForEach(c=>c.VCamera.enabled = c.Type == type);
        }

        private void OnEnable()
        {
            _defaultIntencity = _directionLight.intensity;
            EventBus.Subscribe<CameraShakeEvent>(ShakeEffects, EventBus.EventRegion.GAMEPLAY);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraShakeEvent>(ShakeEffects, EventBus.EventRegion.GAMEPLAY);
        }

        protected void ShakeEffects(CameraShakeEvent gEvent)
        {
            Shake(gEvent.shakeIntensity);
            Dark(gEvent.darkDuration);
        }

        private void Shake(float intensity = 1f)
        {
            if(intensity>0)
            {
                _impulseSource.GenerateImpulse(intensity);
            }
        }
        
        private async void Dark(float duration = 1f)
        {
            if(duration>0)
            {
                _directionLight.intensity = _defaultIntencity * .3f;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity * .2f;
                await UniTask.Delay(TimeSpan.FromSeconds(duration));
                _directionLight.intensity = _defaultIntencity;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity * .2f;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity * .1f;
                await UniTask.Delay(TimeSpan.FromSeconds(.05f));
                _directionLight.intensity = _defaultIntencity;
            }
        }
    }
}