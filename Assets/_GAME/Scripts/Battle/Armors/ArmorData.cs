using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Equipment;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.Battle.Armors
{
    [Serializable]
    public class ArmorEquipmentInfo: EquipmentInfo
    {
        [SerializeField] private int _health;
        [SerializeField] private int _defense;
        [SerializeField] private int _energyShield;

        public int Health => _health;
        public int Defense => _defense;
        public int EnergyShield => _energyShield;
    }

    [Serializable]
    public class ArmorData: IEquipmentData
    {
        [SerializeField] private string _inventoryId;
        [SerializeField] private InventoryCase _inventoryCase = InventoryCase.Armor;

        [SerializeField] private ArmorType _type;

        [SerializeField] private List<ArmorEquipmentInfo> _levels;

        public int Level { get; protected set; } = 0;

        public int MaxLevel => _levels.Count-1;

        public ArmorEquipmentInfo Info => _levels[Mathf.Min(Level, MaxLevel)];
        public ArmorEquipmentInfo NextLevelInfo => _levels[Mathf.Min(Level+1, MaxLevel)];

        public string InventoryId => _inventoryId;
        public InventoryCase InventoryCase => _inventoryCase;
        public ArmorType Type => _type;

        public int Health => Info.Health;
        public int Defense => Info.Defense;
        public int EnergyShield => Info.EnergyShield;

        public void SetLevel(int level)
        {
            Level = level;
        }

        public string GetStatsLabel()
        {
            var label = "Stats:";
            label += $"\nLevel: {Level + 1}/{MaxLevel + 1}";
            label += $"\nHealth: {Health}";
            label += $"\nDefense: {Defense}";
            label += $"\nEnergy shield: {EnergyShield}";
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
            label += Info.Health != NextLevelInfo.Health
                ? $"\nHealth: <color=green>{NextLevelInfo.Health}</color>"
                : $"\nHealth: {Health}";
            label += Info.Defense != NextLevelInfo.Defense
                ? $"\nDefense: <color=green>{NextLevelInfo.Defense}</color>"
                : $"\nDefense: {Defense}";
            label += Info.EnergyShield != NextLevelInfo.EnergyShield
                ? $"\nEnergy shield: <color=green>{NextLevelInfo.EnergyShield}</color>"
                : $"\nEnergy shield: {EnergyShield}";
            return label;
        }
    }
}
