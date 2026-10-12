using System;
using System.Collections.Generic;
using _GAME.Scripts.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    public class WeaponRecoilAnimator: MonoBehaviour
    {
        [SerializeField] private float _recoilSpeed;
        [SerializeField] private List<FromToTransformState> _transformStates;
        [SerializeField] private AnimationCurve _curve;

        private float _time = 1;
        protected float _recoilValue;
        
        public virtual void Play()
        {
            _time = 0;
            _recoilValue = 0;
            foreach (FromToTransformState state in _transformStates)
            {
                state.Apply(0f);
            }
        }

        protected virtual void Update()
        {
            if (_time < 1)
            {
                _time += Time.deltaTime * _recoilSpeed;
                _recoilValue = _curve.Evaluate(_time);
                foreach (FromToTransformState state in _transformStates)
                {
                    state.Apply(_recoilValue);
                }
            }
        }

        public virtual void Stop()
        {
            _time = 1;
            foreach (FromToTransformState state in _transformStates)
            {
                state.Apply(0f);
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("SetFrom")]
        private void SetFrom()
        {
            foreach (FromToTransformState state in _transformStates)
            {
                state.SetFrom();
            }
        }
        
        [ContextMenu("SetTo")]
        private void SetTo()
        {
            foreach (FromToTransformState state in _transformStates)
            {
                state.SetTo();
            }
        }
        #endif
    }
}