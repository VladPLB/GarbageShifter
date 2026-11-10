using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Equipment;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    [Serializable]
    public class SecondaryWeaponEquipmentInfo: EquipmentInfo
    {
        [SerializeField] private int _damage;
        // Amount of main weapon hits needed to fully charge
        [SerializeField] private int _capacity;

        public int Damage => _damage;
        public int Capacity => Mathf.Max(1, _capacity);
    }

    [Serializable]
    public class SecondaryWeaponData: IEquipmentData
    {
        [SerializeField] private string _inventoryId;
        [SerializeField] private InventoryCase _inventoryCase = InventoryCase.SubWeapon;

        [SerializeField] private SecondaryWeaponType _type;

        [SerializeField] private List<SecondaryWeaponEquipmentInfo> _levels;

        public int Level { get; protected set; } = 0;

        public int MaxLevel => _levels.Count-1;

        public SecondaryWeaponEquipmentInfo Info => _levels[Mathf.Min(Level, MaxLevel)];
        public SecondaryWeaponEquipmentInfo NextLevelInfo => _levels[Mathf.Min(Level+1, MaxLevel)];

        public string InventoryId => _inventoryId;
        public InventoryCase InventoryCase => _inventoryCase;
        public SecondaryWeaponType Type => _type;

        public int Damage => Info.Damage;
        public int Capacity => Info.Capacity;

        public void SetLevel(int level)
        {
            Level = level;
        }

        public string GetStatsLabel()
        {
            var label = "Stats:";
            label += $"\nLevel: {Level + 1}/{MaxLevel + 1}";
            label += $"\nDamage: {Damage}";
            label += $"\nCapacity: {Capacity}";
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
            label += Info.Damage != NextLevelInfo.Damage
                ? $"\nDamage: <color=green>{NextLevelInfo.Damage}</color>"
                : $"\nDamage: {Damage}";
            label += Info.Capacity != NextLevelInfo.Capacity
                ? $"\nCapacity: <color=green>{NextLevelInfo.Capacity}</color>"
                : $"\nCapacity: {Capacity}";
            return label;
        }
    }
}
