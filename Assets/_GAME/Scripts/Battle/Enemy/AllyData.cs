using System;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    [Serializable]
    public class AllyData
    {
        [SerializeField] private AllyType _type;
        [SerializeField] private AllyClassType _class;

        [SerializeField] private WeaponData _weaponData;
        
        [SerializeField] private int _health;
        [SerializeField] private int _armor;
        [SerializeField] private float _moveSpeed;

        public AllyType Type => _type;
        public AllyClassType Class => _class;

        public WeaponData WeaponData => _weaponData;
        public int Health => _health;
        public int Armor => _armor;
        public float MoveSpeed => _moveSpeed;
    }
}