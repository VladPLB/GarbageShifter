using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    public class WeaponBomb:MonoBehaviour
    {
        [SerializeField] private WeaponViewer _viewer;

        private ExplosionData _data;
        private Func<bool> _fireHandler;
        private bool _isActive = false;

        public void Setup(ExplosionData data, Func<bool> fireHandler)
        {
            _data = new ExplosionData();
            _data.Setup(data, new ExplosionEffectAttribute());
            _fireHandler = fireHandler;
            _viewer.Setup();
            _viewer.gameObject.SetActive(true);
        }
        
        public void SetActive(bool isActive, bool isForce)
        {
            _isActive = isActive;
            _viewer.gameObject.SetActive(isActive);
            if (_isActive)
            {
                
                _viewer.BattleReady();
            }
            else
            {
                _viewer.BattleStop(isForce);
            }
        }

        public void Explode()
        {
            if(!_isActive)
                return;
            
            SetActive(false, false);
            var explosion = Explosion.Create(_data.Type);
            explosion.transform.position = transform.position;
            explosion.Setup(_data);
        }

        private void Update()
        {
            if(!_isActive)
                return;
            
            if(_fireHandler!=null && _fireHandler.Invoke())
                Explode();
        }
    }
}