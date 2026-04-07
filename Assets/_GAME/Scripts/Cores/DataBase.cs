using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts
{
    public class DataBase: MonoBehaviour, IRuntimeSetup
    {
        [Header("Core")]
        [SerializeField] private ItemDatabase  _itemDatabase;
        [Header("Gameplay")]
        [SerializeField] private EnemyDatabase _enemyDatabase;
        
        [SerializeField] private WeaponDatabase _weaponDatabase;
        [SerializeField] private BulletDatabase _bulletDatabase;
        [SerializeField] private ExplosionDatabase _explosionDatabase;
        
        [Header("Common")]
        [SerializeField] private TextEffectsDatabase _textEffectsDatabase;
        [SerializeField] private GameEffectsDatabase _gameEffectsDatabase;
        [SerializeField] private CoinDatabase _coinDatabase;
        [SerializeField] private UIMarkersDatabase _uiEnemyMarkersDatabase;
        [SerializeField] private UIEnemyHealthBarsDatabase _uiEnemyHealthBarsDatabase;
        [SerializeField] private MapLocationItemsDatabase _mapLocationItemsDatabase;

        public ItemDatabase ItemDatabase => _itemDatabase;
        public EnemyDatabase Enemies => _enemyDatabase;
        public WeaponDatabase Weapons => _weaponDatabase;
        public BulletDatabase Bullets => _bulletDatabase;
        public ExplosionDatabase Explosion => _explosionDatabase;
        
        public TextEffectsDatabase TextsEffects => _textEffectsDatabase;
        public GameEffectsDatabase GameEffects => _gameEffectsDatabase;
        public CoinDatabase Coins => _coinDatabase;
        public UIMarkersDatabase UIEnemyMarkers => _uiEnemyMarkersDatabase;
        public UIEnemyHealthBarsDatabase UIEnemyHealthBars => _uiEnemyHealthBarsDatabase;
        
        public MapLocationItemsDatabase MapLocationItemsDatabase => _mapLocationItemsDatabase;

        private void Awake()
        {
            Core.Registry(this);
        }

        public void RuntimeSetup()
        {
            ItemDatabase.RuntimeSetup();
            Enemies.RuntimeSetup();
            Weapons.RuntimeSetup();
            Bullets.RuntimeSetup();
            Explosion.RuntimeSetup();
            TextsEffects.RuntimeSetup();
            GameEffects.RuntimeSetup();
            Coins.RuntimeSetup();
            UIEnemyMarkers.RuntimeSetup();
            UIEnemyHealthBars.RuntimeSetup();
            MapLocationItemsDatabase.RuntimeSetup();
        }
    }
}