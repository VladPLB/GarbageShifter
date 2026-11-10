using System;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private int _weaponLevel;

        [SerializeField] private int _health;
        [SerializeField] private int _armorLevel;
        [SerializeField] private int _secondaryWeaponLevel;

        public int WeaponLevel => _weaponLevel;
        public int Health => _health;
        public int ArmorLevel => _armorLevel;
        public int SecondaryWeaponLevel => _secondaryWeaponLevel;
    }
}