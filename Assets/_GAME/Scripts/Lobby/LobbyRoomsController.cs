using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
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

        private Dictionary<LobbyPlaceType, LobbyRoom> _lobbyRoomsByType = new();

        private LobbyPlaceType _currentRoomType = LobbyPlaceType.Transition;
        private LobbyPlaceType _targetRoomType = LobbyPlaceType.Transition;
        private LobbyPlaceType _peviousRoomType = LobbyPlaceType.Transition;
        private bool _running = false;
        private bool _initialize = false;

        private MapController _mapController;

        public LobbyPlaceType StartRoom { get; private set; } = LobbyPlaceType.Bar_Barmen;
        public LobbyPlaceType CurrentRoomType
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
            CurrentRoomType = LobbyPlaceType.Transition;
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
            
            EventBus.Subscribe<SetLobbyStartPlaceEvent>(SetLobbyStartPlaceEventHandler, EventBus.EventRegion.LOBBY);
            _initialize = true;
        }

        private void SetLobbyStartPlaceEventHandler(SetLobbyStartPlaceEvent e)
        {
            StartRoom = e.Type;
        }

        public async UniTask OpenTablet()
        {
            _mapController.Show();
            _tabletAnimator.SetTrigger(OpenKey);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _cameraController.SetCamera(LobbyPlaceType.Map);
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

        public async UniTask ToRoom(LobbyPlaceType type)
        {
            if (_running)
                return;

            await UniTask.WaitWhile(() => !_initialize);

            if (type == CurrentRoomType)
                return;

            _running = true;
            _targetRoomType = type;
            if (_targetRoomType == LobbyPlaceType.Map)
            {
                if (_currentRoomType == LobbyPlaceType.Map)
                {
                    await CloseTablet();
                    CurrentRoomType = _targetRoomType = _peviousRoomType;
                }
                else
                {
                    _peviousRoomType = _currentRoomType;
                    CurrentRoomType = type;
                    await OpenTablet();
                    EventBus.Push(new OnLobbyPlaceEvent(CurrentRoomType), EventBus.EventRegion.LOBBY);
                }
            }
            else
            {
                if (_currentRoomType == LobbyPlaceType.Map)
                {
                    await CloseTablet();
                    CurrentRoomType = _peviousRoomType;
                }
                if (CurrentRoomType == LobbyPlaceType.Transition || !_lobbyRoomsByType[CurrentRoomType].Has(type))
                {

                    _cameraController.SetCamera(LobbyPlaceType.Transition);
                    if (CurrentRoomType == LobbyPlaceType.Transition)
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
                    EventBus.Push(new OnLobbyPlaceEvent(CurrentRoomType), EventBus.EventRegion.LOBBY);
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