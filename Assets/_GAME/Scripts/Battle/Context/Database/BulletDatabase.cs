using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "BulletDatabase", menuName = "Scriptable/DB/Bullet/Database", order = 0)]
    public class BulletDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<BulletReferenceData> _bullets = new();
        private Dictionary<BulletType, BulletReferenceData> _bulletByType = new();
        
        public void RuntimeSetup()
        {
            _bulletByType = _bullets.ToDictionary(b => b.Data.Type);
        }

        public Bullet GetPrefab(BulletType type)
        {
            return _bulletByType[type].Prefab;
        }
        
        public BulletData GetDefaultData(BulletType type)
        {
            var reference = _bulletByType[type];
            var data = reference.Data.Clone();
            return data;
        }
    }
}