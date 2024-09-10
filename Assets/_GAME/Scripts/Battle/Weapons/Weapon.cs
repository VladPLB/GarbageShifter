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

        private WeaponData _data;
        private Func<bool> _fireHandler;
        private bool _isActive = false;
        private BulletData _defaultBulletData;

        public void Setup(WeaponData data, Func<bool> fireHandler, Transform freeStateParent = null)
        {
            _data = data;
            _fireHandler = fireHandler;
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
                var bullet = Bullet.Create(_defaultBulletData.Type);
                bullet.transform.position = _barrel.position;
                bullet.transform.forward = _barrel.forward;
                var data = _defaultBulletData.Clone();
                data.Setup(_data.DamageDealer, _data.Damage, _barrel.forward);
                bullet.Setup(data);
            }
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