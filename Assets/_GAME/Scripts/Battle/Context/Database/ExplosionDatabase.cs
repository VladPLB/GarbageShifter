using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "ExplosionDatabase", menuName = "Scriptable/DB/Explosion/Database", order = 0)]
    public class ExplosionDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<Explosion> _items = new();
        private Dictionary<ExplosionType, Explosion> _itemsByType = new();
        
        public void RuntimeSetup()
        {
            _itemsByType = _items.ToDictionary(i => i.Type);
        }

        public Explosion GetPrefab(ExplosionType type)
        {
            return _itemsByType[type];
        }
    }
}