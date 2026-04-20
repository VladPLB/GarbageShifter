using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Audio;
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
        private Dictionary<ItemInfo, Vector2Int> _dropItemTypes = new();
        
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
            _dropItemTypes.Clear();
            var datas = _inventoryManager.GetAll(ItemType.Material, false);
            foreach (var data in datas)
            {
                var mul = (1f / ((int)data.Rank * 5f));
                if (data.SubType == "Scrap")
                {
                    var scrapCountRange = new Vector2Int((int)(40f*mul), (int)(80f*mul));
                    _dropItemTypes.Add(data, scrapCountRange);
                }
                else if (data.SubType == "Tools")
                {
                    var toolsCountRange = new Vector2Int((int)(20f*mul), (int)(30f*mul));
                    _dropItemTypes.Add(data, toolsCountRange);
                }
            }
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
            
            var info = _dropItemTypes.Keys.ElementAt(Random.Range(0, _dropItemTypes.Count));
            var count = Random.Range(_dropItemTypes[info].x, _dropItemTypes[info].y);
            _localInventory.Add(info, count);
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