using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Player.SecondaryWeapon;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "SecondaryWeaponDatabase", menuName = "Scriptable/DB/SecondaryWeapon/Database", order = 0)]
    public class SecondaryWeaponDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<SecondaryWeaponReferenceData> _weapons = new();
        private Dictionary<SecondaryWeaponType, SecondaryWeaponReferenceData> _weaponByType = new();
        private Dictionary<string, SecondaryWeaponReferenceData> _weaponByInventoryId = new();

        public void RuntimeSetup()
        {
            _weaponByType = _weapons.ToDictionary(w => w.Data.Type);
            _weaponByInventoryId = _weapons.ToDictionary(w => w.Data.InventoryId);
        }

        public SecondaryWeaponData GetData(SecondaryWeaponType type)
        {
            var reference = _weaponByType[type];
            var data = reference.Data.Clone();
            return data;
        }

        public SecondaryWeaponData GetData(string inventoryId)
        {
            if (!_weaponByInventoryId.TryGetValue(inventoryId, out var reference))
            {
                return null;
            }

            var data = reference.Data.Clone();
            return data;
        }

        public SecondaryWeaponBase GetPrefab(SecondaryWeaponType type)
        {
            return _weaponByType[type].Prefab;
        }
    }
}
