using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Armors;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "ArmorDatabase", menuName = "Scriptable/DB/Armor/Database", order = 0)]
    public class ArmorDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<ArmorReferenceData> _armors = new();
        private Dictionary<ArmorType, ArmorReferenceData> _armorByType = new();
        private Dictionary<string, ArmorReferenceData> _armorByInventoryId = new();

        public void RuntimeSetup()
        {
            _armorByType = _armors.ToDictionary(a => a.Data.Type);
            _armorByInventoryId = _armors.ToDictionary(a => a.Data.InventoryId);
        }

        public ArmorData GetData(ArmorType type)
        {
            var reference = _armorByType[type];
            var data = reference.Data.Clone();
            return data;
        }

        public ArmorData GetData(string inventoryId)
        {
            if (!_armorByInventoryId.TryGetValue(inventoryId, out var reference))
            {
                return null;
            }

            var data = reference.Data.Clone();
            return data;
        }

        public GameObject GetPrefab(ArmorType type)
        {
            return _armorByType[type].Prefab;
        }
    }
}
