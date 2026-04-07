using System;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
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
        private Action _onFire;

        public void Setup(WeaponData data, Func<bool> fireHandler, Action onHit = null, Transform freeStateParent = null, Action fireCallback = null)
        {
            _data = data;
            _fireHandler = fireHandler;
            _onHit = onHit;
            _onFire = fireCallback;
            _defaultBulletData = Core.Get<DataBase>().Bullets.GetDefaultData(_data.BulletType);
            
            if(freeStateParent!=null)
            {
                _viewer.Setup(transform, freeStateParent);
            }
            else
            {
                _viewer.Setup();
            }
        }
        
        public void SetActive(bool isActive, bool isForce)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _viewer.BattleReady();
            }
            else
            {
                _viewer.BattleStop(isForce);
            }
        }

        public void TryFire()
        {
            if (_data.TryFireRegister())
            {
                Fire();
            }
        }

        public async void Fire()
        {
            _onFire?.Invoke();
            for(int i =0;i<_data.BulletAmount;i++)
            {
                _muzzleFlash?.Play();
                EventBus.Push(new SoundPlayEvent(SoundType.Shot, _barrel.position), EventBus.EventRegion.GLOBAL);
                var bullet = Bullet.Create(_defaultBulletData.Type);
                bullet.transform.position = _barrel.position;
                var offset = (Mathf.PerlinNoise1D(Time.time) * 2f - 1f) * _data.AimOffset;
                bullet.transform.forward = (_barrel.forward + _barrel.right * offset + _barrel.up * offset).normalized;
                var data = _defaultBulletData.Clone();
                data.Setup(_data.DamageDealer, _data.DamageRandomize, bullet.transform.forward);
                bullet.Setup(data, OnHit);
                bullet.gameObject.SetActive(true);
                if (i < _data.BulletAmount - 1 && _data.BulletSpawnDelay>0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Abs(_data.BulletSpawnDelay)));
                }
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