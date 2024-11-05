using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class ExplosionDeadHandler : DeadHandler
    {
        [SerializeField] private ExplosionType _explosionType;
        [SerializeField] private Transform _point;
        public override void OnDead()
        {
            var item = Explosion.Create(_explosionType);
            item.transform.position = _point.position;
            item.Setup();
        }
    }
}