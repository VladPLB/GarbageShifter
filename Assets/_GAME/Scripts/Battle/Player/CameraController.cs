using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _GAME.Scripts.Battle.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private List<VCameraByType> _cameras;
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        [SerializeField] private Light _globalLight;
        
        private Volume _volume = null;
        private ChromaticAberration _chromaticAberration = null;
        private Sequence _glitchSequence = null;
        
        public void SetCamera(GameCameraType type)
        {
            _cameras.ForEach(c=>c.VCamera.enabled = c.Type == type);
        }

        [Serializable]
        public struct VCameraByType
        {
            public GameCameraType Type;
            public CinemachineVirtualCamera VCamera;
        }
        
        void Start()
        {
            _volume = FindObjectOfType<Volume>();
            if(_volume!=null)
            {
                _volume.profile.TryGet(out _chromaticAberration);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CameraShakeEvent>(ShakeEffects, EventBus.EventRegion.GAMEPLAY);
        }

        private void OnDisable()
        {
            _volume = null;
            _chromaticAberration = null;
            EventBus.Unsubscribe<CameraShakeEvent>(ShakeEffects, EventBus.EventRegion.GAMEPLAY);
        }

        protected void ShakeEffects(CameraShakeEvent gEvent)
        {
            ShakeEffects(gEvent.shakeIntensity, gEvent.glitchIntensity, gEvent.duration);
        }

        public void ShakeEffects(float shakeIntensity = 0, float glitchIntensity = 0, float duration = 0.2f)
        {
            if(shakeIntensity>0)
            {
                Shake(shakeIntensity);
            }
            if(glitchIntensity>0)
            {
                EnableGlitch(glitchIntensity, duration);
            }
        }
        
        private void Shake(float intensity = 1f)
        {
            _impulseSource.GenerateImpulse(intensity);
        }

        private void EnableGlitch(float intensity = 0.5f, float duration = 0.2f)
        {
            if(_volume ==null || _chromaticAberration == null)
                return;
            if(_glitchSequence.IsPlaying())
            {
                _glitchSequence.Kill();
            }
            
            _chromaticAberration.active = true;
            _glitchSequence = DOTween.Sequence()
                .Append(DOTween.To(() => _chromaticAberration.intensity.value,
                    (v) => _chromaticAberration.intensity.value = v, intensity, duration * .5f).SetEase(Ease.InExpo))
                .AppendInterval(duration * .2f)
                .Append(DOTween.To(() => _chromaticAberration.intensity.value,
                    (v) => _chromaticAberration.intensity.value = v, 0, duration * .3f).SetEase(Ease.InCirc))
                .AppendCallback(() => _chromaticAberration.active = false)
                .Play();
        }
    }
}