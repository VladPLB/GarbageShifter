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
        [SerializeField] private bool _autoRevive = false;
        [SerializeField] private List<Collider> _colliders = new();
        [SerializeField] private List<GameObject> _activeItems = new();
        [SerializeField] private List<GameObject> _destroyedItems = new();
        [SerializeField] private List<DeadHandler> _deadHandlers = new();
        [SerializeField] private GameEffectType _deadEffectType = GameEffectType.Explosion_Small;
        [SerializeField] private Vector3 _deadEffectOffset = Vector3.zero;

        public override Team Team => Team.None;

        private Vector3? _lastHitPoint = null;

        private void Start()
        {
            _health.OnChangeValue += OnHealthChange;
            Revive();
        }

        private void Revive()
        {
            _health.Reset();
            gameObject.SetActive(true);
            _activeItems.ForEach(a => a.SetActive(true));
            _destroyedItems.ForEach(a => a.SetActive(false));
            _colliders.ForEach(a => a.enabled = true);
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
            _deadHandlers.ForEach(h=>h.OnDead());
            if (_deadEffectType != GameEffectType.None)
            {
                GameEffect.Create(_deadEffectType, transform.position + _deadEffectOffset);
            }
            _activeItems.ForEach(a => a.SetActive(false));
            _destroyedItems.ForEach(a => a.SetActive(true));
            _colliders.ForEach(a => a.enabled = false);
            if(_autoRevive)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(2));
                gameObject.SetActive(false);
                Revive();
            }
        }
    }
}