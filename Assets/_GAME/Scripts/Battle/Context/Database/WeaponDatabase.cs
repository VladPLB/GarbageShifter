using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Scriptable/DB/Weapon/Database", order = 0)]
    public class WeaponDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<WeaponReferenceData> _weapons = new();
        private Dictionary<WeaponType, WeaponReferenceData> _weaponByType = new();
        
        public void RuntimeSetup()
        {
            _weaponByType = _weapons.ToDictionary(w => w.Data.Type);
        }
        
        public WeaponData GetData(WeaponType type)
        {
            var reference = _weaponByType[type];
            var data = reference.Data.Clone();
            return data;
        }
    }
}