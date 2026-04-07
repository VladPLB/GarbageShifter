using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<ItemInfo> _items = new();

        private Dictionary<string, ItemInfo> _byId;
        private Dictionary<ItemType, List<ItemInfo>> _byType;

        public IReadOnlyList<ItemInfo> Items => _items;

        public bool TryGet(string id, out ItemInfo item)
        {
            return _byId.TryGetValue(id, out item);
        }

        public List<ItemInfo> GetAll()
        {
            var keys = _byType.Keys.ToList();
            var items = new List<ItemInfo>();

            foreach (var key in keys)
            {
                items.AddRange(_byType[key]);
            }
            
            return items;
        }

        public List<ItemInfo> GetByType(ItemType type)
        {
            return _byType.TryGetValue(type, out var items) ? items : new List<ItemInfo>();
        }

        public List<ItemInfo> GetByTypeSubType(ItemType type, string subType)
        {
            var items = GetByType(type);
            
            return items
                .Where(i =>
                    i != null &&
                    !string.IsNullOrWhiteSpace(i.SubType) &&
                    string.Equals(i.SubType, subType,  StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void RuntimeSetup()
        {
            _byId = new Dictionary<string, ItemInfo>(StringComparer.Ordinal);

            foreach (var item in _items)
            {
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                _byId[item.Id] = item;
            }
            
            _byType = _items
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }

}