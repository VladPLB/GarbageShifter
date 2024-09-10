using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Scriptable/DB/Enemy/Database", order = 0)]
    public class EnemyDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<EnemyReferenceData> _enemies = new();
        private Dictionary<EnemyType, EnemyReferenceData> _enemyByType = new();
        private Dictionary<EnemyType, EnemyClassType> _classByType = new();
        private Dictionary<EnemyType, EnemySubClassType> _subClassByType = new();

        public void RuntimeSetup()
        {
            _enemyByType = _enemies.ToDictionary(w =>
            {
                _classByType.Add(w.Data.Type, w.Data.Class);
                _subClassByType.Add(w.Data.Type, w.Data.SubClass);
                return w.Data.Type;
            });

        }
        
        public EnemyController GetPrefab(EnemyType type)
        {
            var reference = _enemyByType[type];
            var pref = reference.Prefab;
            return pref;
        }

        public EnemyClassType GetClass(EnemyType type)
        {
            if (_classByType.ContainsKey(type))
                return _classByType[type];
            return EnemyClassType.None;
        }
        
        public EnemySubClassType GetSubClass(EnemyType type)
        {
            if (_classByType.ContainsKey(type))
                return _subClassByType[type];
            return EnemySubClassType.Default;
        }
    }
}