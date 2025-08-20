using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Lobby;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyBottomPanel : LobbyPanel
    {
        [SerializeField] private List<BottomPanelRoomButton> _roomButtons;

        [SerializeField] private RoomPlacesButtons _placesButtonsHolder;

        private LobbyRoomsController _roomsController;
        private bool _isAnimate = false;
        private List<LobbyPlaceType> _types;

        public async UniTask Initialize(LobbyRoomsController roomsController)
        {
            _roomsController = roomsController;
            _holderTransform.anchoredPosition = _hidePosition;
            _placesButtonsHolder.Setup();
            foreach (var roomButton in _roomButtons)
            {
                roomButton.Setup(OnRoomClick);
            }
            EventBus.Subscribe<ToLobbyPlaceEvent>(ToLobbyPlaceEventHandler, EventBus.EventRegion.LOBBY);
            Show().Forget();
            await ToStartRoom();
        }

        private void ToLobbyPlaceEventHandler(ToLobbyPlaceEvent e)
        {
            ToRoom(e.Type, true).Forget();
        }

        private async UniTask Select(LobbyPlaceType type)
        { 
            if(_isAnimate)
                return;
            
            _types = GetTypes(type);
            _isAnimate = true;
            
            foreach (var roomButton in _roomButtons)
            {
                roomButton.Select(type);
            }
            
            _placesButtonsHolder.TryHide(type);
            _placesButtonsHolder.Select(type);
            await _roomsController.ToRoom(type);
            _placesButtonsHolder.Show(_types, type, OnPlaceClick);
            _isAnimate = false;
        }

        private List<LobbyPlaceType> GetTypes(LobbyPlaceType type)
        {
            foreach (var roomButton in _roomButtons)
            {
                if (roomButton.Types.Contains(type))
                {
                    return roomButton.Types;
                }
            }

            return new() { type };
        }

        private async UniTask ToStartRoom()
        {
            Debug.Log($"ToStartRoom {_roomsController.StartRoom}");
            await ToRoom(_roomsController.StartRoom);
        }
        
        private async UniTask ToRoom(LobbyPlaceType type, bool awaitAnimate = false)
        {
            if (_isAnimate)
            {
                if (!awaitAnimate)
                {
                    return;
                }
                else
                {
                    await UniTask.WaitWhile(() => _isAnimate == true);
                }
            }
            await Select(type);
        }

        private void OnRoomClick(List<LobbyPlaceType> types)
        {
            ToRoom(types[0]).Forget();
        }

        private void OnPlaceClick(LobbyPlaceType type)
        {
            ToRoom(type).Forget();
        }
    }
}