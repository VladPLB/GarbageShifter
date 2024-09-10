using System;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class Player: MonoBehaviour, IDamageDealer
    {
        public uint ID => 0;
        public Team Team => Team.Player;
        
        public event Action OnDestinationTargetPosition;

        [SerializeField] private PlayerViewer _viewer;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private Transform _freeWeaponHolder;
        
        private CameraController _cameraController;
        private GameInput _gameInput;
        private bool _battleReady = false;
        
        
        public void Setup(CameraController cameraController)
        {
            _cameraController = cameraController;
            _gameInput = new GameInput();
            _aim.Setup(_gameInput);
            SetupWeapon();
        }

        private void SetupWeapon()
        {
            var weaponData = Core.Get<DataBase>().Weapons.GetDefaultData(WeaponType.Riffle);
            weaponData.Setup(this);
            _weapon.Setup(weaponData, IsFire, _freeWeaponHolder);
        }

        private bool IsFire()
        {
            return _gameInput.Game.Fire.IsPressed();
        }

        public void BattleReady()
        {
            _aim.SetActive(true);
            _weapon.SetActive(true);

            _gameInput.Enable();
            _battleReady = true;
            _cameraController.SetCamera(GameCameraType.Battle);
        }

        public void BattleStop()
        {
            _aim.SetActive(false);
            _weapon.SetActive(false);

            _gameInput.Enable();
            _battleReady = false;
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
            _mover.MoveTo(targetPosition.Position, targetPosition.Rotation);
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
    }
}