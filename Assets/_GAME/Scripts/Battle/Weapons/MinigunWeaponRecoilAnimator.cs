using System;
using System.Collections.Generic;
using _GAME.Scripts.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    public class MinigunWeaponRecoilAnimator: WeaponRecoilAnimator
    {
        [SerializeField] protected Transform _barrelTransform;
        [SerializeField] protected Vector3 _barrelRotate;
        [SerializeField] protected float _speedFactor;

        protected bool _isRotate = false;
        protected float _speed = 0;
        
        
        public override void Play()
        {
            _isRotate = true;
            base.Play();
        }

        public void PlayRotate()
        {
            _isRotate = true;
        }

        protected override void Update()
        {
            if (_isRotate)
            {
                if (_speed < 1f)
                {
                    _speed += Time.deltaTime * _speedFactor;
                }
            }
            else
            {
                if (_speed > 0)
                {
                    _speed -= Time.deltaTime * _speedFactor * 2f;
                }
            }
            
            _speed = Mathf.Clamp(_speed, 0f, 1f);
            if (_speed > 0)
            {
                _barrelTransform.Rotate(_barrelRotate * _speed);
            }
            base.Update();
        }

        public void StopRotate()
        {
            _isRotate = false;
        }

        public override void Stop()
        {
            _isRotate = false;
            base.Stop();
        }
    }
}