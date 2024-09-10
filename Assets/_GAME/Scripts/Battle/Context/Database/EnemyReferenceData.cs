using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Weapons;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "EnemyReference_", menuName = "Scriptable/DB/Enemy/Reference", order = 1)]
    public class EnemyReferenceData : ScriptableObject
    {
        [SerializeField] private EnemyController _prefab;

        public EnemyData Data => _prefab.Data;
        public EnemyController Prefab => _prefab;
    }
}