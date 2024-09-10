using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "BulletReference_", menuName = "Scriptable/DB/Bullet/Reference", order = 1)]
    public class BulletReferenceData : ScriptableObject
    {
        [SerializeField] private BulletData _data;
        [SerializeField] private Bullet _prefab;

        public BulletData Data => _data;
        public Bullet Prefab => _prefab;
    }
}