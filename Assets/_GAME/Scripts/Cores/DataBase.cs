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
        [SerializeField] private ExplosionDatabase _explosionDatabase;
        
        [Header("Common")]
        [SerializeField] private TextEffectsDatabase _textEffectsDatabase;
        [SerializeField] private GameEffectsDatabase _gameEffectsDatabase;
        [SerializeField] private UIMarkersDatabase _uiEnemyMarkersDatabase;
        [SerializeField] private UIEnemyHealthBarsDatabase _uiEnemyHealthBarsDatabase;
        [SerializeField] private MapLocationItemsDatabase _mapLocationItemsDatabase;

        public EnemyDatabase Enemies => _enemyDatabase;
        public WeaponDatabase Weapons => _weaponDatabase;
        public BulletDatabase Bullets => _bulletDatabase;
        public ExplosionDatabase Explosion => _explosionDatabase;
        
        public TextEffectsDatabase TextsEffects => _textEffectsDatabase;
        public GameEffectsDatabase GameEffects => _gameEffectsDatabase;
        public UIMarkersDatabase UIEnemyMarkers => _uiEnemyMarkersDatabase;
        public UIEnemyHealthBarsDatabase UIEnemyHealthBars => _uiEnemyHealthBarsDatabase;
        
        public MapLocationItemsDatabase MapLocationItemsDatabase => _mapLocationItemsDatabase;

        private void Awake()
        {
            Core.Registry(this);
        }

        public void RuntimeSetup()
        {
            Enemies.RuntimeSetup();
            Weapons.RuntimeSetup();
            Bullets.RuntimeSetup();
            Explosion.RuntimeSetup();
            TextsEffects.RuntimeSetup();
            GameEffects.RuntimeSetup();
            UIEnemyMarkers.RuntimeSetup();
            UIEnemyHealthBars.RuntimeSetup();
            MapLocationItemsDatabase.RuntimeSetup();
        }
    }
}