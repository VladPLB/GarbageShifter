using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class AllyController : DamageReceiver, IPoolableItem<AllyType>, IDamageDealer
    {
        [SerializeField] private AllyData _data;
        [SerializeField] private UnitViewer _viewer;
        [SerializeField] private BaseRagdollBehaviour _ragDoll;
        [SerializeField] private AllyMover _mover;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private BaseAllyTargetFinder _targetFinder;
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        [SerializeField] private StateIntAttribute _health;
        [SerializeField] private StateIntAttribute _armor;
        [SerializeField, ReadOnly] private DamageRepeater[] _damageRepeaters;
        [SerializeField] private GameEffectType _deadEffectType = GameEffectType.None;
        [SerializeField] private Vector3 _deadEffectOffset = Vector3.zero;
        private Vector3? _lastHitPoint = null;
        private Vector3? _explosionForce = null;
        private DamageTextType _lastDamageTextType;

        private bool _isFire = false;
        public override Team Team => Team.Ally;
        public AllyType Type => _data.Type;
        public AllyData Data => _data;
        public AllyMover Mover => _mover;
        public StateIntAttribute Health => _health;

        public event Action<AllyController> OnDead;

        public void Init()
        {
            _data.WeaponData.Setup(this, 1);
            _weapon.Setup(_data.WeaponData, IsFire, fireCallback:_viewer.Fire);
            
            _health.OnChangeValue -= OnHealthChange;
            _health.OnChangeValue += OnHealthChange;
            
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
            }
        }

        public void Setup(Vector3 spawnPoint, Vector3 targetPoint)
        {
            Init();
            _lastHitPoint = null;
            _explosionForce = null;
            _targetFinder.Init(this);
            _mover.Setup(spawnPoint, targetPoint, _data.MoveSpeed, _targetFinder.GetTarget);
            if(_ragDoll!=null)
            {
                _ragDoll.Setup();
            }
            _viewer.Setup();
            _weapon.SetActive(true, false);

            _health.Set(_data.Health);
            _armor.Set(_data.Armor);
            DamageRepeatersSetActive(true);
        }

        public void Play()
        {
            _mover.Play();
            _mover.OnMoveSpeed += _viewer.Run;
        }

        private bool IsFire()
        {
            return _targetFinder.IsFire();
        }
        
        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            if(damageDealersTeam == Team.Enemy)
                return;
            
            _lastHitPoint = hitPoint;
            _lastDamageTextType = DamageTextType.Default;
            
            var dmg = damageAmount;
            if (additiveAttributes.Find(a => a.Key == EffectAttributeType.ExplosionDamage) != null)
            {
                _lastHitPoint = null;
                _explosionForce = (transform.position - hitPoint).normalized;
                dmg = Mathf.RoundToInt(dmg * GameConstants.EXPLOSION_DAMAGE_MULTIPLIER);
                _lastDamageTextType = DamageTextType.Explosion;
            }
            if (additiveAttributes.Find(a => a.Key == EffectAttributeType.HeadShot) != null)
            {
                dmg = Mathf.RoundToInt(dmg * GameConstants.HEADSHOT_DAMAGE_MULTIPLIER);
                _lastDamageTextType = DamageTextType.Headshot;
            }
            if (additiveAttributes.Find(a => a.Key == EffectAttributeType.ShieldShot) != null)
            {
                if (_armor.Current > 0)
                {
                    dmg = Mathf.RoundToInt(dmg * GameConstants.ARMOR_DAMAGE_MULTIPLIER);
                    _armor.Remove(damageAmount);
                    _lastDamageTextType = DamageTextType.Armor;
                }
            }
            if (additiveAttributes.Find(a => a.Key == EffectAttributeType.WeakShot) != null)
            {
                dmg = Mathf.RoundToInt(dmg * GameConstants.WEAK_DAMAGE_MULTIPLIER);
                _lastDamageTextType = DamageTextType.Weak;
            }
            
            _health.Remove(damageAmount);
            _lastHitPoint = null;
            _explosionForce = null;
        }
        
        public void OnHealthChange(int delta)
        {
            _damageReactionViewer?.Show(delta, _lastHitPoint ?? transform.position, _lastDamageTextType);
            
            _lastDamageTextType = DamageTextType.Default;

            if (_health.Current <= 0)
            {
                Dead();
            }
            else
            {
                _viewer.Hit();
            }
        }

        private void DamageRepeatersSetActive(bool isActive)
        {
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.SetActive(isActive);
            }
        }

        public void Deactivate(bool isForce)
        {
            _weapon.SetActive(false, isForce);
            _mover.Deactivate();
            DamageRepeatersSetActive(false);
            OnDead = null;
        }

        public void Dead()
        {
            if (_ragDoll != null)
            {
                if (_explosionForce != null)
                {
                    _ragDoll.ShowWithExplosion(_explosionForce.Value, 3).Forget();
                }
                else if(_lastHitPoint!=null)
                {
                    _ragDoll.ShowWithHit(_lastHitPoint.Value, 8f).Forget();
                }
                else
                {
                    _ragDoll.Show().Forget();
                }
            }
            else
            {
                _viewer.Dead();
            }

            if (_deadEffectType != GameEffectType.None)
            {
                GameEffect.Create(_deadEffectType, transform.position + _deadEffectOffset);
            }
            OnDead?.Invoke(this);
            Deactivate(false);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("ApplyAllDamageRepeaters")]
        private void ApplyAllDamageRepeaters()
        {
            _damageRepeaters = GetComponentsInChildren<DamageRepeater>();
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
                damageRepeater.FindColliders();
            }
        }
        #endif
        
    }
}