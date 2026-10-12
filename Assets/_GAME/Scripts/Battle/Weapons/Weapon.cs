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
    public class Weapon:MonoBehaviour, IPoolableItem<WeaponType>
    {
        public WeaponType Type => _type;
        
        [SerializeField] private WeaponType _type;
        [SerializeField] private WeaponViewer _viewer;
        [SerializeField] protected WeaponRecoilAnimator _recoilAnimator;
        [SerializeField] private bool _soundByShot = true;
        [SerializeField] private List<Transform> _barrels;
        [SerializeField] private List<ParticleSystem> _muzzleFlashes;
        [SerializeField] private Transform _leftIkAnchor;
        [SerializeField] private Transform _rightIkAnchor;

        private WeaponData _data;
        private BulletData _defaultBulletData;
        private Action _onHit;
        private Action _onFire;
        private int _barrelIndex = 0;
        
        protected Func<bool> _fireHandler;
        protected bool _isActive = false;

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
            SetActive(false, true);
        }

        public (Transform left, Transform right) GetIkAnchors()
        {
            return (_leftIkAnchor, _rightIkAnchor);
        }
        
        public virtual void SetActive(bool isActive, bool isForce)
        {
            _isActive = isActive;
            _recoilAnimator?.Stop();

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
            var ct = this.GetCancellationTokenOnDestroy();
            try
            {
                _onFire?.Invoke();
                _recoilAnimator?.Play();
                if (!_soundByShot)
                {
                    EventBus.Push(new SoundPlayEvent(_data.SoundType, _barrels[0].position), EventBus.EventRegion.GLOBAL);
                    _muzzleFlashes[0]?.Play();
                }
                for(int i =0;i<_data.BulletAmount;i++)
                {
                    var barrel =  _barrels[_barrelIndex];
                    var muzzleFlash = _muzzleFlashes[_barrelIndex];
                    _barrelIndex++;
                    if (_barrelIndex >= _barrels.Count)
                    {
                        _barrelIndex = 0;
                    }
                    if (_soundByShot)
                    {
                        muzzleFlash?.Play();
                        EventBus.Push(new SoundPlayEvent(_data.SoundType, barrel.position),
                            EventBus.EventRegion.GLOBAL);
                    }
                    var bullet = Bullet.Create(_defaultBulletData.Type);
                    bullet.transform.position = barrel.position;
                    var offsetX = Random.Range(-1f, 1f) * _data.AimOffset;
                    var offsetY = offsetX * Random.Range(-1f, 1f);
                    bullet.transform.forward = (barrel.forward + barrel.right * offsetX + barrel.up * offsetY).normalized;
                    var data = _defaultBulletData.Clone();
                    data.Setup(_data.DamageDealer, _data.DamageRandomize, bullet.transform.forward);
                    bullet.Setup(data, OnHit);
                    bullet.gameObject.SetActive(true);
                    if (i < _data.BulletAmount - 1 && _data.BulletSpawnDelay>0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Abs(_data.BulletSpawnDelay)), cancellationToken: ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // component destroyed mid-await, expected
            }
        }

        private void OnHit()
        {
            _onHit?.Invoke();
        }


        protected virtual void Update()
        {
            if(!_isActive)
                return;
            
            if(_fireHandler!=null && _fireHandler.Invoke())
                TryFire();
        }
        
        public static Weapon Create(WeaponType type)
        {
            return Core.Get<PoolProvider>().Weapons.Pop(type);
        }

        public static void Release(Weapon weapon)
        {
            Core.Get<PoolProvider>().Weapons.Push(weapon);
        }
    }
}