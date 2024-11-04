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
    public class EnemyController : DamageReceiver, IPoolableItem<EnemyType>, IDamageDealer
    {
        [SerializeField] private EnemyData _data;
        [SerializeField] private EnemyViewer _enemyViewer;
        [SerializeField] private RagDollController _ragDoll;
        [SerializeField] private EnemyMover _enemyMover;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        [SerializeField] private Transform _healthBarPoint;
        [SerializeField] private StateIntAttribute _health;
        [SerializeField] private StateIntAttribute _armor;
        [SerializeField, ReadOnly]
        private DamageRepeater[] _damageRepeaters;
        private Vector3? _lastHitPoint = null;
        private DamageTextType _lastDamageTextType;
        private Transform _playerTransform;

        private bool _isFire = false;
        public override Team Team => Team.Enemy;
        public EnemyType Type => _data.Type;
        public EnemyData Data => _data;
        public StateIntAttribute Health => _health;

        public Vector3 HealthBarPoint => _healthBarPoint.position;

        public event Action<EnemyController> OnDead;

        public void Awake()
        {
            _data.WeaponData.Setup(this, 1);
            _weapon.Setup(_data.WeaponData, IsFire);
            _health.OnChangeValue += OnHealthChange;
            
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
            }
        }

        public void Setup(List<Vector3> movePath, Transform player, EnemyBounds enemyBounds)
        {
            
            _playerTransform = player;
            
            _enemyMover.Setup(movePath, _playerTransform, enemyBounds, _data.MoveSpeed, _data.AttackDistance);
            _ragDoll.Setup();
            _enemyViewer.Setup();
            _weapon.SetActive(true, false);
            _health.Set(_data.Health);
            _armor.Set(_data.Armor);
            DamageRepeatersSetActive(true);
        }

        public void Play()
        {
            _enemyMover.Play();
            _enemyMover.OnMoveSpeed += _enemyViewer.Run;
        }

        public bool IsFire()
        {
            if (!_enemyMover.AttackedDistance)
            {
                _isFire = false;
                return false;
            }

            if (!_isFire)
            {
                _data.WeaponData.ResetFireTime(Time.time);
            }

            _isFire = true;
            return true;
        }
        
        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            if(damageDealersTeam == Team.Enemy)
                return;
            _lastHitPoint = hitPoint;
            _lastDamageTextType = DamageTextType.Default;
            
            var dmg = damageAmount;
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
                _enemyViewer.Hit();
            }
            _lastHitPoint = null;
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
            _enemyMover.Deactivate();
            DamageRepeatersSetActive(false);
            OnDead = null;
        }

        public void Dead()
        {
            if (_ragDoll != null)
            {
                if(_lastHitPoint!=null)
                {
                    _ragDoll.Show(_lastHitPoint ?? transform.position + transform.forward, 10f).Forget();
                }
                else
                {
                    _ragDoll.Show().Forget();
                }
            }
            else
            {
                _enemyViewer.Dead();
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