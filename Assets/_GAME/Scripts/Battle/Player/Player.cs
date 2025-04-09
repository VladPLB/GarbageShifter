using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Player.SecondaryWeapon;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using Unity.Collections;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class Player: DamageReceiver, IDamageDealer
    {
        [SerializeField] private PlayerData _data;
        [SerializeField] private PlayerViewer _viewer;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private SecondaryWeaponController _secondaryWeaponController;
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private Transform _freeWeaponHolder;
        [SerializeField] private StateIntAttribute _health;
        
        [SerializeField, ReadOnly]
        private DamageRepeater[] _damageRepeaters;
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        
        private CameraController _cameraController;
        private GameInput _gameInput;
        private bool _battleReady = false;
        private bool _previousFireState = false;

        private bool _isAimLock = false;
        
        public override Team Team => Team.Player;
        public StateIntAttribute Health => _health;

        public bool IsReadySecondaryWeapon => _secondaryWeaponController != null && _secondaryWeaponController.IsReady;
        public float SecondaryWeaponProgress => _secondaryWeaponController != null? _secondaryWeaponController.Progress: 0f;
        
        public event Action OnDestinationTargetPosition;
        public event Action<bool> OnBattleReady;
        public event Action OnHit;
        public event Action OnShot;
        
        public event Action OnSeccondaryShot;

        public event Action OnDamaged;

        private void Awake()
        {
            _health.OnChangeValue += OnHealthChange;
        }

        public void Setup(CameraController cameraController)
        {
            _cameraController = cameraController;
            _gameInput = new GameInput();
            _aim.Setup(_gameInput);
            SetupWeapon();
            _secondaryWeaponController.Setup(this, _aim);
            _health.Set(_data.Health);
            
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
            }
            
            EventBus.Subscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
        }

        private void SetupWeapon()
        {
            var weaponData = Core.Get<DataBase>().Weapons.GetData(WeaponType.Riffle);
            weaponData.Setup(this, _data.WeaponLevel);
            _weapon.Setup(weaponData, IsFire, OnHitHandler, _freeWeaponHolder, OnFire);
            OnHit += _secondaryWeaponController.OnHit;
            OnShot += _viewer.Fire;
        }

        private void OnHitHandler()
        {
            OnHit?.Invoke();
            
        }
        
        private void OnFire()
        {
            OnShot?.Invoke();
        }

        private void OnAimLock(AimLockEvent lockEvent)
        {
            _isAimLock = lockEvent.IsLock;
        }

        public bool IsFire()
        {
            var state = _gameInput.Game.Fire.IsPressed() && !_isAimLock;
            if (_previousFireState != state)
            {
                if (!state)
                {
                    _secondaryWeaponController.OnFire();
                }
            }

            _previousFireState = state;
            return state;
        }

        public void BattleReady()
        {
            _aim.SetActive(true);
            _weapon.SetActive(true, false);

            _gameInput.Enable();
            _battleReady = true;
            _cameraController.SetCamera(GameCameraType.Battle);
            OnBattleReady?.Invoke(true);
        }

        public void BattleStop()
        {
            _aim.SetActive(false);
            _weapon.SetActive(false, false);

            _gameInput.Disable();
            _battleReady = false;
            OnBattleReady?.Invoke(false);
        }

        private void OnReactToDestinationPosition()
        {
            _viewer.Stop();
            OnDestinationTargetPosition?.Invoke();
            OnDestinationTargetPosition = null;
        }

        public void SetPosition(PlayerPosition positionSource)
        {
            _cameraController.SetCamera(GameCameraType.Run);
            _mover.TeleportTo(positionSource.Position, positionSource.Rotation);
        }
        
        public void MoveToPosition(PlayerPosition previewPosition, PlayerPosition targetPosition)
        {
            _cameraController.SetCamera(GameCameraType.Run);
            _mover.OnMoveCompleted += OnReactToDestinationPosition;
            _mover.OnFly += _viewer.Fly;
            _mover.MoveTo(targetPosition.Position, targetPosition.Rotation, previewPosition.Type!= PlayerPositionType.Start);
            PlayAnimationByPositionType(previewPosition.Type);
        }

        public void Victory()
        {
            PlayAnimationByPositionType(PlayerPositionType.End);
        }
        
        private void PlayAnimationByPositionType(PlayerPositionType type)
        {
            Action viewerAction = type switch
            {
                PlayerPositionType.Start => _viewer.Run,
                PlayerPositionType.Default => _viewer.Jump,
                PlayerPositionType.End => _viewer.Victory,
                _ => _viewer.Run
            };
            viewerAction.Invoke();
        }
        
        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            OnDamaged?.Invoke();
            _health.Remove(damageAmount);
        }
        
        public void OnHealthChange(int delta)
        {
            _damageReactionViewer?.Show(delta, transform.position);

            if (_health.Current <= 0)
            {
               // Dead(); TODO
            }
        }

        private void OnDisable()
        {
            OnBattleReady = null;
            OnHit = null;
            OnShot = null;
            OnSeccondaryShot = null;
            OnDestinationTargetPosition = null;
            OnDamaged = null;
            EventBus.Unsubscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
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