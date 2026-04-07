using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Common;
using _GAME.Scripts.Cores.Save.SavesConfigs;
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
        
        PoolProvider _poolProvider;
        private SaveManager _saveManager;
        private ProgressData _progressData;
        
        Pool<Coin, CoinType> _pool;
        

        private void Awake()
        {
            Core.Registry(this, typeof(PoolProvider), typeof(SaveManager));
        }
    
        public void RuntimeSetup()
        {
            _poolProvider = Core.Get<PoolProvider>();
            _saveManager = Core.Get<SaveManager>();
            _progressData = _saveManager.GetData<ProgressData>();
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
                var type = CoinType.Metal;// Extentions.GetRandom<CoinType>();
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
            if (Random.Range(0, 100) < 25)
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