using System;
using _GAME.Scripts.Battle.Context;
using UnityEngine;

namespace _GAME.Scripts
{
    public class DataBase: MonoBehaviour, IRuntimeSetup
    {
        [SerializeField] private EnemyDatabase _enemyDatabase;
        
        [SerializeField] private WeaponDatabase _weaponDatabase;
        [SerializeField] private BulletDatabase _bulletDatabase;
        
        [Header("Common")]
        [SerializeField] private TextEffectsDatabase _textEffectsDatabase;
        [SerializeField] private GameEffectsDatabase _gameEffectsDatabase;

        public EnemyDatabase Enemies => _enemyDatabase;
        public WeaponDatabase Weapons => _weaponDatabase;
        public BulletDatabase Bullets => _bulletDatabase;
        
        public TextEffectsDatabase TextsEffects => _textEffectsDatabase;
        public GameEffectsDatabase GameEffects => _gameEffectsDatabase;

        private void Awake()
        {
            Core.Registry(this);
        }

        public void RuntimeSetup()
        {
            Enemies.RuntimeSetup();
            Weapons.RuntimeSetup();
            Bullets.RuntimeSetup();
            TextsEffects.RuntimeSetup();
            GameEffects.RuntimeSetup();
        }
    }
}