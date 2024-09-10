using System;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    [Serializable]
    public class WeaponData: DamageDealerData
    {
        [SerializeField] private WeaponType _type;
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private float _reloadTime;
        [SerializeField] private int _damage;

        public int Level { get; protected set; } = 1;
        public float LastFireTime { get; protected set; } = 0;
        
        public WeaponType Type => _type;
        public BulletType BulletType => _bulletType;
        public float ReloadTime => _reloadTime;
        public int Damage => _damage;

        public void Setup(IDamageDealer damageDealer, int level)
        {
            base.Setup(damageDealer);
            Level = level;
        }

        public bool TryFireRegister()
        {
            var timeNow = Time.time;
            if (timeNow < LastFireTime + ReloadTime)
            {
                return false;
            }

            LastFireTime = timeNow;
            return true;
        }
    }
}