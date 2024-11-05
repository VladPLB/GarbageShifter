using System;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    [Serializable]
    public class EnemyData
    {
        [SerializeField] private EnemyType _type;
        [SerializeField] private EnemyClassType _class;
        [SerializeField] private EnemySubClassType _subClass;

        [SerializeField] private WeaponData _weaponData;
        [SerializeField] private bool _isBomber = false;
        [SerializeField] private ExplosionData _explosionData;

        [SerializeField] private int _health;
        [SerializeField] private int _armor;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _attackDistance;


        public EnemyType Type => _type;
        public EnemyClassType Class => _class;
        public EnemySubClassType SubClass => _subClass;

        public WeaponData WeaponData => _weaponData;
        public bool IsBomber => _isBomber;
        public ExplosionData ExplosionData => _explosionData;
        public int Health => _health;
        public int Armor => _armor;
        public float MoveSpeed => _moveSpeed;
        public float AttackDistance => _attackDistance;
    }
}