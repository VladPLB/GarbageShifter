using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Map;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyRoomsController : MonoBehaviour
    {
        private static readonly int OpenKey = Animator.StringToHash("Open");
        private static readonly int CloseKey = Animator.StringToHash("Close");
        
        [SerializeField] private LobbyCameraController _cameraController;
        [SerializeField] private List<LobbyRoom> _rooms;
        [SerializeField] private Animator _rotatorAnimator;
        [SerializeField] private Animator _tabletAnimator;
        [SerializeField] private Animator _mapCameraAnimator;

        private Dictionary<LobbyCameraType, LobbyRoom> _lobbyRoomsByType = new();

        private LobbyCameraType _currentRoomType = LobbyCameraType.Transition;
        private LobbyCameraType _targetRoomType = LobbyCameraType.Transition;
        private LobbyCameraType _peviousRoomType = LobbyCameraType.Transition;
        private bool _running = false;
        private bool _initialize = false;

        private MapController _mapController;

        public LobbyCameraType CurrentRoomType
        {
            get => _currentRoomType;
            set
            {
                _currentRoomType = value;
            }
        }

        public void Initialize(MapController mapController)
        {
            _running = false;
            _mapController = mapController;
            CurrentRoomType = LobbyCameraType.Transition;
            _lobbyRoomsByType.Clear();
            foreach (var room in _rooms)
            {
                foreach (var type in room.Types)
                {
                    if (!_lobbyRoomsByType.ContainsKey(type))
                    {
                        _lobbyRoomsByType.Add(type, room);
                    }
                }
            }
            
            _initialize = true;
        }

        public async UniTask OpenTablet()
        {
            _mapController.Show();
            _tabletAnimator.SetTrigger(OpenKey);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _cameraController.SetCamera(LobbyCameraType.Map);
            _mapCameraAnimator.SetTrigger(OpenKey);
            await UniTask.Delay(TimeSpan.FromSeconds(.6f));
        }
        
        public async UniTask CloseTablet()
        {
            _cameraController.SetCamera(_peviousRoomType);
            _mapCameraAnimator.SetTrigger(CloseKey);
            await UniTask.Delay(TimeSpan.FromSeconds(.6f));
            _tabletAnimator.SetTrigger(CloseKey);
            _mapController.Hide();
            await UniTask.Delay(TimeSpan.FromSeconds(.6f));
        }

        public async UniTask ToRoom(LobbyCameraType type)
        {
            if (_running)
                return;

            await UniTask.WaitWhile(() => !_initialize);

            if (type == CurrentRoomType)
                return;

            _running = true;
            _targetRoomType = type;
            if (_targetRoomType == LobbyCameraType.Map)
            {
                if (_currentRoomType == LobbyCameraType.Map)
                {
                    await CloseTablet();
                    CurrentRoomType = _targetRoomType = _peviousRoomType;
                }
                else
                {
                    _peviousRoomType = _currentRoomType;
                    CurrentRoomType = type;
                    await OpenTablet();
                }
            }
            else
            {
                if (_currentRoomType == LobbyCameraType.Map)
                {
                    await CloseTablet();
                    CurrentRoomType = _peviousRoomType;
                }
                if (CurrentRoomType == LobbyCameraType.Transition || !_lobbyRoomsByType[CurrentRoomType].Has(type))
                {

                    _cameraController.SetCamera(LobbyCameraType.Transition);
                    if (CurrentRoomType == LobbyCameraType.Transition)
                    {
                        _rotatorAnimator.Rebind();
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(1.1f));
                        _rotatorAnimator.SetTrigger(CloseKey);
                        await UniTask.Delay(TimeSpan.FromSeconds(.5f));
                    }

                    CurrentRoomType = type;
                    foreach (var room in _rooms)
                    {
                        room.SetRoomActive(room.Has(_targetRoomType));
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(.2f));
                    _rotatorAnimator.SetTrigger(OpenKey);
                    await UniTask.Delay(TimeSpan.FromSeconds(.5f));
                    _cameraController.SetCamera(_targetRoomType);
                    await UniTask.Delay(TimeSpan.FromSeconds(.5f));
                }
                else
                {
                    _cameraController.SetCamera(_targetRoomType);
                    CurrentRoomType = type;
                    await UniTask.Delay(TimeSpan.FromSeconds(.5f));
                }
            }
            
            _running = false;
        }
    }
}