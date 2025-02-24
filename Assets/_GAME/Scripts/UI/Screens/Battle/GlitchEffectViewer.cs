using System;
using _GAME.Scripts.Events;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class GlitchEffectViewer : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _activeColor;

        [SerializeField] private float _forceModificator = 1f;
        [SerializeField] private int _vibration = 10;

        private ScreenshotCapturer _screenshotCapturer;
        private Color _inactiveColor;
        
        public void Setup()
        {
            EventBus.Subscribe<CameraShakeEvent>(OnGlitch, EventBus.EventRegion.GAMEPLAY);
            _screenshotCapturer = Core.Get<ScreenshotCapturer>();
            _inactiveColor = _activeColor;
            _inactiveColor.a = 0;
            _image.color = _inactiveColor;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraShakeEvent>(OnGlitch, EventBus.EventRegion.GAMEPLAY);
            _image.color = _inactiveColor;
        }

        private void OnGlitch(CameraShakeEvent gEvent)
        {
            if(gEvent.glitchIntensity>0 && gEvent.duration > 0)
            {
                _screenshotCapturer.TakeScreenshot((t)=>ShowGlitch(t, gEvent.glitchIntensity, gEvent.duration));
            }
        }

        private void ShowGlitch(Texture2D tex, float force, float duration)
        {
            _image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f);
            _image.color = _activeColor;
            DOTween.Sequence()
                //.Append(_image.transform.DOScale(Vector3.one * 1.1f, duration * .6f ))
                .Append(_image.DOColor(_activeColor, duration * .6f))
                .Join(_image.transform.DOShakePosition(duration * .6f, force * _forceModificator, _vibration).SetEase(Ease.OutCirc))
                .Append(_image.DOColor(_inactiveColor, duration * .4f))
                //.Join(_image.transform.DOScale(Vector3.one, duration * .4f ))
                .Play();
        }
    }
}