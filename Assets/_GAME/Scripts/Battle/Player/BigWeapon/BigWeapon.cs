using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class BigWeapon:MonoBehaviour
    {
        [SerializeField] WeaponData _data;
        [SerializeField] BulletData _bulletData;
        [SerializeField] private BigWeaponAim _aim;
        [SerializeField] private Player _player;
        [SerializeField] private Transform _cameraFollowTransform;
        [SerializeField] private Transform _cameraTargetTransform;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _armIkL;
        [SerializeField] private Transform _armIkR;
        [SerializeField] private List<WeaponGattlingBarrelAnimator> _barrelAnimators;
        [SerializeField] private List<Transform> _barrelPoints;
        [Header("Recoil Settings")]
        [SerializeField] private Transform _recoilTransform;
        [SerializeField] private Transform _recoilForwardTransform;
        [SerializeField] private float _recoilForce = 0.5f;
        [SerializeField] private float _recoilReturnSpeed = 5f;
        [SerializeField] private float _maxRecoilDistance = 0.2f;

        private Vector3 _originalLocalPosition;
        private float _currentRecoilForce;

        private bool _isActive = false;

        
        private Func<bool> _fireHandler;
        private Action _onHit;

        public BigWeaponAim Aim => _aim;
        
        public Transform Target => _target;
        public Transform CameraFollowTransform => _cameraFollowTransform;
        public Transform CameraTargetTransform => _cameraTargetTransform;
        
        public Transform ArmIkL => _armIkL;
        public Transform ArmIkR => _armIkR;

        public void SetActive(bool b)
        {
            _isActive = b;
            _aim.SetActive(_isActive);
            if (_isActive)
            {
                _originalLocalPosition = _recoilTransform.localPosition;
                _currentRecoilForce = 0;
            }

        }

        public void Setup(Player player, Func<bool> fireHandler, Action onHit = null)
        {
            _data.Setup(player);
            _fireHandler = fireHandler;
            _onHit = onHit;
        }

        public void TryFire()
        {
            if (_barrelAnimators[0].IsFireReady && _data.TryFireRegister())
            {
                Fire();
            }
        }

        public async void Fire()
        {
            int barelIndex = 0;
            for(int i =0;i<_data.BulletAmount;i++)
            {
                barelIndex = (barelIndex + 1) % _barrelPoints.Count;
                EventBus.Push(new SoundPlayEvent(SoundType.Shot, _barrelPoints[barelIndex].position), EventBus.EventRegion.GLOBAL);
                
                ApplyRecoil();
                
                var bullet = Bullet.Create(_bulletData.Type);
                bullet.transform.position = _barrelPoints[barelIndex].position;
                var offset = (Mathf.PerlinNoise1D(Time.time) * 2f - 1f) * _data.AimOffset;
                bullet.transform.forward = (_barrelPoints[barelIndex].forward + _barrelPoints[barelIndex].right * offset + _barrelPoints[barelIndex].up * offset).normalized;
                var data = _bulletData.Clone();
                data.Setup(_data.DamageDealer, _data.DamageRandomize, bullet.transform.forward);
                bullet.Setup(data, OnHit);
                bullet.gameObject.SetActive(true);
                if (i < _data.BulletAmount - 1 && _data.BulletSpawnDelay>0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Abs(_data.BulletSpawnDelay)));
                }
            }
        }
        
        private void ApplyRecoil()
        {
            _currentRecoilForce = Mathf.Min(_currentRecoilForce + _recoilForce, _maxRecoilDistance);
        }


        private void OnHit()
        {
            _onHit?.Invoke();
        }


        private void Update()
        {
            if(!_isActive)
                return;

            bool isFire = _fireHandler != null && _fireHandler.Invoke();
            foreach (var barrelAnimator in  _barrelAnimators)
            {
                barrelAnimator.IsFire = isFire;
            }
            if (isFire)
            {
                TryFire();
            }
            
            if (_currentRecoilForce > 0.001f)
            {
                _currentRecoilForce = Mathf.Lerp(_currentRecoilForce, 0f, Time.deltaTime * _recoilReturnSpeed);
                _recoilTransform.localPosition = _originalLocalPosition + Vector3.forward * _currentRecoilForce * -1f;
            }
            else
            {
                _currentRecoilForce = 0f;
                _recoilTransform.localPosition = _originalLocalPosition;
            }

        }
    }
}