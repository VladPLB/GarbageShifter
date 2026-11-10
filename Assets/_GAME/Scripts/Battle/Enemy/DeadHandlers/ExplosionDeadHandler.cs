using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class ExplosionDeadHandler : DeadHandler, IBulletPayloadHandler
    {
        [SerializeField] private ExplosionType _explosionType;
        [SerializeField] private Transform _point;
        public override void OnDead()
        {
            var item = Explosion.Create(_explosionType);
            item.transform.position = _point.position;
            item.Setup();
        }

        public void OnDead(BulletData data)
        {
            var item = Explosion.Create(_explosionType);
            item.transform.position = _point.position;
            item.Setup(data.DamageDealer, data.PayloadDamage);
        }
    }
}