using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
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
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private Transform _freeWeaponHolder;
        [SerializeField] private StateIntAttribute _health;
        
        [SerializeField, ReadOnly]
        private DamageRepeater[] _damageRepeaters;
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        
        private CameraController _cameraController;
        private GameInput _gameInput;
        private bool _battleReady = false;
        
        public override Team Team => Team.Player;
        public StateIntAttribute Health => _health;
        
        public event Action OnDestinationTargetPosition;
        public event Action<bool> OnBattleReady;
        public event Action OnHit;

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
            _health.Set(_data.Health);
            
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
            }
        }

        private void SetupWeapon()
        {
            var weaponData = Core.Get<DataBase>().Weapons.GetData(WeaponType.Riffle);
            weaponData.Setup(this, _data.WeaponLevel);
            _weapon.Setup(weaponData, IsFire, OnHitHandler, _freeWeaponHolder);
        }

        private void OnHitHandler()
        {
            OnHit?.Invoke();
        }

        public bool IsFire()
        {
            return _gameInput.Game.Fire.IsPressed();
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

            _gameInput.Enable();
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
            OnDestinationTargetPosition = null;
            OnDamaged = null;
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