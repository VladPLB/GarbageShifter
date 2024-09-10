using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Enemy
{
    public class DestroyablePropsItem : DamageReceiver
    {
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        [SerializeField] private StateIntAttribute _health;
        [SerializeField] private GameEffectType _deadEffect;
        [SerializeField] private Transform _deadEffectPoint;
        public override Team Team => Team.None;

        private Vector3? _lastHitPoint = null;

        private void Start()
        {
            _health.OnChangeValue += OnHealthChange;
            Revive();
        }

        private void Revive()
        {
            _health.Set(Random.Range(20, 40));
            gameObject.SetActive(true);
        }

        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            if(damageDealersTeam == Team.Enemy)
                return;

            _lastHitPoint = hitPoint;
            _health.Remove(damageAmount);
        }

        public void OnHealthChange(int delta)
        {
            _damageReactionViewer?.Show(delta, _lastHitPoint ?? transform.position);
            _lastHitPoint = null;
            
            if (_health.Current <= 0)
            {
                Dead();
            }
        }

        public async void Dead()
        {
            GameEffect.Create(_deadEffect, _deadEffectPoint.position);
            gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            Revive();
        }
    }
}