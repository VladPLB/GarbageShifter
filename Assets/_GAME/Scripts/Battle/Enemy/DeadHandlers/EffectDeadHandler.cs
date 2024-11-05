using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class EffectDeadHandler : DeadHandler
    {
        [SerializeField] private GameEffectType _deadEffect;
        [SerializeField] private Transform _deadEffectPoint;
        public override void OnDead()
        {
            GameEffect.Create(_deadEffect, _deadEffectPoint.position);
        }
    }
}