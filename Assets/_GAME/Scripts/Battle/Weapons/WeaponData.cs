using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Equipment;
using _GAME.Scripts.Common;
using _GAME.Scripts.Inventory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Weapons
{
    [Serializable]
    public class EquipmentInfo
    {
        
    }

    [Serializable]
    public class WeaponEquipmentInfo: EquipmentInfo
    {
        [SerializeField] private float _reloadTime;
        [SerializeField] private float _reloadTimeRandomize;
        [SerializeField] private float _bulletAmount = 1;
        [SerializeField] private float _bulletSpawnDelay = 0;
        [SerializeField] private int _damage;
        [SerializeField] private int _damageOffset;
        [SerializeField] private float _aimOffset;
        
        public float ReloadTime => _reloadTime;
        public float ReloadTimeRandomize => _reloadTimeRandomize;
        public float BulletAmount => _bulletAmount;
        public float BulletSpawnDelay => _bulletSpawnDelay;
        public int Damage => _damage;
        public int DamageOffset => _damageOffset;
        public int DamageRandomize => _damage + (int)Random.Range(-_damageOffset, _damageOffset);
        public float AimOffset => _aimOffset;
    }
    [Serializable]
    public class WeaponData: DamageDealerData, IEquipmentData
    {
        [SerializeField] private string _inventoryId;
        [SerializeField] private InventoryCase _inventoryCase;
        
        [SerializeField] private WeaponType _type;
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private ExplosionType _explosionType;
        [SerializeField] private SoundType _soundType;

        [SerializeField] private List<WeaponEquipmentInfo> _levels;

        public int Level { get; protected set; } = 0;
        
        public int MaxLevel => _levels.Count-1;
        
        public WeaponEquipmentInfo Info => _levels[Mathf.Min(Level, MaxLevel)];
        public WeaponEquipmentInfo NextLevelInfo => _levels[Mathf.Min(Level+1, MaxLevel)];
        
        public float LastFireTime { get; protected set; } = 0;
        // Runtime damage scale (enemy progression), not saved in configs
        public float DamageMultiplier { get; protected set; } = 1f;
        
        public string InventoryId => _inventoryId;
        public InventoryCase InventoryCase => _inventoryCase;
        public WeaponType Type => _type;
        public BulletType BulletType => _bulletType;
        public ExplosionType ExplosionType => _explosionType;
        
        public SoundType SoundType => _soundType;
        
        public float ReloadTime => Info.ReloadTime;
        public float ReloadTimeRandomize => Info.ReloadTimeRandomize;
        public float BulletAmount => Info.BulletAmount;
        public float BulletSpawnDelay => Info.BulletSpawnDelay;
        public int Damage => Mathf.RoundToInt(Info.Damage * DamageMultiplier);
        
        public int DamageOffset =>  Info.DamageOffset;
        public int DamageRandomize => Mathf.RoundToInt(Info.DamageRandomize * DamageMultiplier);
        public float AimOffset => Info.AimOffset;

        public void Setup(IDamageDealer damageDealer, int level)
        {
            base.Setup(damageDealer);
            Level = level;
        }

        public void SetLevel(int level)
        {
            Level = level;
        }

        public void SetDamageMultiplier(float multiplier)
        {
            DamageMultiplier = Mathf.Max(0f, multiplier);
        }

        public bool TryFireRegister()
        {
            var timeNow = Time.time;
            if (timeNow < LastFireTime)
            {
                return false;
            }

            ResetFireTime(timeNow);
            return true;
        }

        public void ResetFireTime(float timeNow)
        {
            LastFireTime = timeNow + ReloadTime + Random.Range(-ReloadTimeRandomize, ReloadTimeRandomize);
        }

        public string GetStatsLabel()
        {
            var label = "Stats:";
            label += $"\nLevel: {Level + 1}/{MaxLevel + 1}";
            label += $"\nReload: {ReloadTime}";
            label += $"\nReload offset: {ReloadTimeRandomize}";
            label += $"\nBullets: {BulletAmount}";
            if (BulletSpawnDelay > 0)
            {
                label += $"\nShot Delay: {BulletSpawnDelay}";
            }
            label += $"\nDamage: {Damage}";
            label += $"\nDamage offset: {DamageOffset}";
            label += $"\nAimOffset: {AimOffset}";
            return label;
        }

        public string GetStatsLabelForUpgrade()
        {
            if (Level >= MaxLevel)
            {
                return "Max Level";
            }

            var label = "Stats:";
            label += $"\nLevel: {Level +1} >> <color=green>{Level + 2}</color>/";
            label += Info.ReloadTime != NextLevelInfo.ReloadTime?
                $"\nReload: <color=green>{NextLevelInfo.ReloadTime}</color>"
                : $"\nReload: {ReloadTime}";
            label += Info.ReloadTimeRandomize != NextLevelInfo.ReloadTimeRandomize? 
                $"\nReload offset: <color=green>{NextLevelInfo.ReloadTimeRandomize}</color>"
                : $"\nReload offset: {ReloadTimeRandomize}";
            label += Info.BulletAmount != NextLevelInfo.BulletAmount? 
                $"\nBullets: <color=green>{NextLevelInfo.BulletAmount}</color>" 
                : $"\nBullets: {BulletAmount}";
            if(Info.BulletSpawnDelay>0 || NextLevelInfo.BulletSpawnDelay > 0)
            {
                label += Info.BulletSpawnDelay != NextLevelInfo.BulletSpawnDelay
                    ? $"\nShot Delay: <color=green>{NextLevelInfo.BulletSpawnDelay}</color>"
                    : $"\nShot Delay: {BulletSpawnDelay}";
            }
            label += Info.Damage != NextLevelInfo.Damage
                ? $"\nDamage: <color=green>{NextLevelInfo.Damage}</color>"
                : $"\nDamage: {Damage}";
            label += Info.DamageOffset != NextLevelInfo.DamageOffset
                ? $"\nDamage offset: <color=green>{NextLevelInfo.DamageOffset}</color>"
                : $"\nShot offset: {DamageOffset}";
            label += Info.AimOffset != NextLevelInfo.AimOffset
                ? $"\nAimOffset: <color=green>{NextLevelInfo.AimOffset}</color>"
                : $"\nAimOffset: {AimOffset}";
            return label;
        }
    }
}