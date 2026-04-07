using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "CoinDatabase", menuName = "Scriptable/DB/Other/CoinDatabase", order = 0)]
    public class CoinDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<Coin> _items = new();
        private Dictionary<CoinType, Coin> _itemByType = new();
        
        public void RuntimeSetup()
        {
            _itemByType = _items.ToDictionary(b => b.Type);
        }

        public Coin GetPrefab(CoinType type)
        {
            return _itemByType[type];
        }
    }
}