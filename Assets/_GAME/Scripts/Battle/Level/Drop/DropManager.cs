using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Level.Progression;
using _GAME.Scripts.Common;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Save;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Items
{
    public class DropManager : MonoBehaviour, IRuntimeSetup, IReparentIgnored
    {
        [SerializeField] private int MaxCoins = 50;
        
        private List<Coin> _activeCoins = new List<Coin>();
        private int _firstActiveCoinIndex = 0;
        private Transform _player;
        private bool _isAttractionActive = false;
        
        private PoolProvider _poolProvider;
        private SaveManager _saveManager;
        private InventoryManager _inventoryManager;
        
        private ProgressData _progressData;
        private InventoryData _inventoryData;
        
        [SerializeField]
        private LocalInventory _localInventory;
        
        private Pool<Coin, CoinType> _pool;
        private readonly Dictionary<(string subType, ItemRank rank), ItemInfo> _materials = new();
        private readonly Dictionary<ItemRank, ItemInfo> _licenses = new();
        private LootProgressionSettings _lootSettings;
        private int? _grade;
        private int? _ownedEquipmentRank;
        
        public LocalInventory LocalInventory => _localInventory;

        private void Awake()
        {
            Core.Registry(this, typeof(PoolProvider), typeof(SaveManager));
        }
    
        public void RuntimeSetup()
        {
            _poolProvider = Core.Get<PoolProvider>();
            _saveManager = Core.Get<SaveManager>();
            _inventoryManager = Core.Get<InventoryManager>();
            _progressData = _saveManager.GetData<ProgressData>();
            _inventoryData = _saveManager.GetData<InventoryData>();
            _localInventory = new LocalInventory(_inventoryData.GameplayStorageCapacity);
            _materials.Clear();
            foreach (var data in _inventoryManager.GetAll(ItemType.Material, true))
            {
                _materials[(data.SubType, data.Rank)] = data;
            }
            _licenses.Clear();
            foreach (var data in _inventoryManager.GetAll(ItemType.License, true))
            {
                _licenses[data.Rank] = data;
            }
            _grade = null;
            _ownedEquipmentRank = null;
            _pool = _poolProvider.Coins;
            Setup(FindObjectOfType<Player.Player>().transform);
        }
        
        private void Setup(Transform player)
        {
            _player = player;
        }
        
        private void SpawnCoin(CoinType type, Vector3 position, int index, int total)
        {
            if(_progressData.Level<1) return;
            
            var coin = _pool.Pop(type);
            
            float angle = (360f / total) * index;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            coin.gameObject.SetActive(true);
            coin.Initialize(position, direction);
            coin.OnCollected += OnCoinCollected;
            
            _activeCoins.Add(coin);
        }

        public void DropCoins(Vector3 position, int amount = -1)
        {
            int coinsCount = amount > 0 ? amount : 1;
            
            for (int i = 0; i < coinsCount; i++)
            {
                var type = CoinType.Metal;
                SpawnCoin(type, position, i, coinsCount);
            }

            if (_activeCoins.Count >= MaxCoins)
            {
                var delta = _activeCoins.Count - MaxCoins;
                for (int i = _firstActiveCoinIndex; i < delta; i++)
                {
                    _activeCoins[i].Hide();
                }
            }
            
            DropLoot();
        }

        // Settings and location are resolved lazily: both are registered after this module
        private LootProgressionSettings LootSettings
        {
            get
            {
                if (_lootSettings == null)
                {
                    var battle = Core.Get<Settings>()?.Battle;
                    _lootSettings = battle != null ? battle.LootProgression : null;
                }
                return _lootSettings;
            }
        }

        private int Grade
        {
            get
            {
                if (_grade == null && LootSettings != null)
                {
                    var progression = Core.Get<LevelController>()?.EnemyProgression;
                    _grade = LootSettings.GetGrade(progression?.Location ?? 0);
                }
                return _grade ?? 0;
            }
        }

        // Per enemy kill: one material stack + a small chance of licenses of every rank up to the grade
        private void DropLoot()
        {
            var settings = LootSettings;
            if (settings == null)
                return;

            var grade = Grade;
            if (Random.value < settings.MaterialDropChance)
            {
                var isTool = Random.value < settings.ToolShare;
                var rank = settings.GetRandomMaterialRank(grade);
                if (_materials.TryGetValue((isTool ? "Tool" : "Scrap", rank), out var material))
                {
                    _localInventory.Add(material, settings.GetMaterialAmount(isTool, rank, grade));
                }
            }

            for (int r = 0; r <= grade; r++)
            {
                var rank = (ItemRank)r;
                if (_licenses.TryGetValue(rank, out var license) && Random.value < settings.GetLicenseChance(rank, grade))
                {
                    _localInventory.Add(license, 1);
                }
            }

            // extra Tools and Licenses above the grade so better equipment owned early can still be upgraded
            for (int r = grade + 1; r <= OwnedEquipmentRank; r++)
            {
                var rank = (ItemRank)r;
                if (_materials.TryGetValue(("Tool", rank), out var tool) && Random.value < settings.GetOwnedRankToolChance(rank, grade))
                {
                    _localInventory.Add(tool, settings.GetMaterialAmount(true, rank, r));
                }
                if (_licenses.TryGetValue(rank, out var license) && Random.value < settings.GetOwnedRankLicenseChance(rank, grade))
                {
                    _localInventory.Add(license, 1);
                }
            }
        }

        // Highest rank of the owned equipment (-1 if none), resolved once per level
        private int OwnedEquipmentRank
        {
            get
            {
                if (_ownedEquipmentRank == null)
                {
                    var owned = _inventoryManager.GetAll(true)
                        .Where(item => item is UpgradableItemInfo && _inventoryManager.GetCount(item) > 0)
                        .Select(item => (int)item.Rank)
                        .DefaultIfEmpty(-1);
                    _ownedEquipmentRank = owned.Max();
                }
                return _ownedEquipmentRank.Value;
            }
        }

        // Guaranteed license of the grade rank on the last level of every N-th location
        public void OnLevelCompleted(int location, bool isLastLevelInLocation)
        {
            var settings = LootSettings;
            if (settings == null || !isLastLevelInLocation || !settings.IsGuaranteedLicenseLocation(location))
                return;

            if (_licenses.TryGetValue((ItemRank)settings.GetGrade(location), out var license))
            {
                _localInventory.Add(license, 1);
            }
        }

        public void StartAttraction()
        {
            _isAttractionActive = false;
            
            foreach (var coin in _activeCoins)
            {
                if (coin != null)
                {
                    coin.StartAttraction(_player);
                }
            }

            _firstActiveCoinIndex = 0;
        }

        private void OnCoinCollected(Coin coin)
        {
            _activeCoins.Remove(coin);
            _pool.Push(coin);
            coin.OnCollected -= OnCoinCollected;
            
            if (Random.Range(0, 100) < 12)
            {
                AudioManager.Play(SoundType.Coin);
            }
        }

        private void OnDestroy()
        {
            Core.Unregistry(this);
        }
    }
}