using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    public class Weapon:MonoBehaviour
    {
        [SerializeField] private WeaponViewer _viewer;
        [SerializeField] private Transform _barrel;
        [SerializeField] private ParticleSystem _muzzleFlash;

        private WeaponData _data;
        private Func<bool> _fireHandler;
        private bool _isActive = false;
        private BulletData _defaultBulletData;
        private Action _onHit; 

        public void Setup(WeaponData data, Func<bool> fireHandler, Action onHit = null, Transform freeStateParent = null)
        {
            _data = data;
            _fireHandler = fireHandler;
            _onHit = onHit;
            _defaultBulletData = Core.Get<DataBase>().Bullets.GetDefaultData(_data.BulletType);
            if(freeStateParent!=null)
            {
                _viewer.SetupOverrideParents(transform, freeStateParent);
            }
        }
        
        public void SetActive(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _viewer.BattleReady();
            }
            else
            {
                _viewer.BattleStop();
            }
        }

        public void TryFire()
        {
            if (_data.TryFireRegister())
            {
                _muzzleFlash.Play();
                var bullet = Bullet.Create(_defaultBulletData.Type);
                bullet.transform.position = _barrel.position;
                var offset = (Mathf.PerlinNoise1D(Time.time) * 2f - 1f) * _data.AimOffset;
                bullet.transform.forward = (_barrel.forward + _barrel.right * offset + _barrel.up * offset).normalized;
                var data = _defaultBulletData.Clone();
                data.Setup(_data.DamageDealer, _data.Damage, bullet.transform.forward);
                bullet.Setup(data, OnHit);
            }
        }

        private void OnHit()
        {
            _onHit?.Invoke();
        }


        private void Update()
        {
            if(!_isActive)
                return;
            
            if(_fireHandler!=null && _fireHandler.Invoke())
                TryFire();
        }
    }
}