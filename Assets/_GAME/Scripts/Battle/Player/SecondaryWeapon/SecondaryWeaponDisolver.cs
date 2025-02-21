using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    public class SecondaryWeaponDisolver : MonoBehaviour
    {
        private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        
        [SerializeField] private List<Renderer> _renderers;
        [SerializeField] private float _duration = 1f;

        private List<Material> _materials = null;
        private float _progress = 0;
        private float _from = 0;
        private float _to = 0;
        private bool _isAnimate = false;
        private float _animateTime = 0f;

        private Action _onEndAnimate;

        public void Setup()
        {
            _materials ??= _renderers.Select(s => s.material).ToList();
            _from = 0;
            _to = 1;
            UpdateProgress(0);
        }
        
        private void Update()
        {
            if (_isAnimate)
            {
                _animateTime += Time.deltaTime * 1f / _duration;
                UpdateProgress(_animateTime);
                if (_animateTime >= 1f)
                {
                    _isAnimate = false;
                    _onEndAnimate?.Invoke();
                    _onEndAnimate = null;
                }
            }
        }

        public void Show(Action animateCallback = null)
        {
            _onEndAnimate = animateCallback;
            _from = 0f;
            _to = 1f;
            StartAnimate();
        }

        public void Hide(Action animateCallback = null)
        {
            _onEndAnimate = animateCallback;
            _from = 1f;
            _to = 0f;
            StartAnimate();
        }

        private void StartAnimate()
        {
            _isAnimate = true;
            _animateTime = 0f;
        }

        private void UpdateProgress(float time)
        {
            _progress = Mathf.Lerp(_from, _to, time);
            foreach (var material in _materials)
            {
                material.SetFloat(DissolveAmount, _progress);
            }
            
        }
    }
}