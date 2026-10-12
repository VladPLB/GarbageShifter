using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Pools;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Weapons
{
    public class WeaponDelayed:Weapon
    {
        [SerializeField] private float _delayDuration;

        private float _delayTime = 0f;
        private bool _isRotate = false;
        
        public override void SetActive(bool isActive, bool isForce)
        {
            base.SetActive(isActive, isForce);
            _delayTime = _delayDuration;
            _isRotate = false;
        }

        protected override void Update()
        {
            if(!_isActive)
                return;
            
            if(_fireHandler!=null && _fireHandler.Invoke())
            {
                if (_delayTime > 0)
                {
                    if(!_isRotate)
                    {
                        _isRotate = true;
                        if (_recoilAnimator != null)
                        {
                            if (_recoilAnimator is MinigunWeaponRecoilAnimator minigunWeaponRecoilAnimator)
                            {
                                minigunWeaponRecoilAnimator.PlayRotate();
                            }
                        }
                    }
                    _delayTime -= Time.deltaTime;
                }
                else
                {
                    TryFire();
                }
            }
            else
            {
                if (_delayTime < _delayDuration)
                {
                    _delayTime += Time.deltaTime * 2f;
                    if(_isRotate)
                    {
                        _isRotate = false;
                        if (_recoilAnimator != null)
                        {
                            if (_recoilAnimator is MinigunWeaponRecoilAnimator minigunWeaponRecoilAnimator)
                            {
                                minigunWeaponRecoilAnimator.StopRotate();
                            }
                        }
                    }
                }
            }
            
            _delayTime =  Mathf.Clamp(_delayTime, 0f, _delayDuration);
        }
    }
}