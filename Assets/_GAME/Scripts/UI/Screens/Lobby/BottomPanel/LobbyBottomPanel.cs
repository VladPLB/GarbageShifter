using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Lobby;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyBottomPanel : MonoBehaviour
    {
        private const LobbyCameraType DefaultRoomType = LobbyCameraType.Bar_Barmen;
        
        [SerializeField] 
        private List<BottomPanelRoomButton> _roomButtons;

        [SerializeField] private RoomPlacesButtons _placesButtonsHolder;

        private LobbyRoomsController _roomsController;
        private bool _isAnimate = false;
        private List<LobbyCameraType> _types;

        public async UniTask Initialize(LobbyRoomsController roomsController)
        {
            _roomsController = roomsController;
            _placesButtonsHolder.Setup();
            foreach (var roomButton in _roomButtons)
            {
                roomButton.Setup(OnRoomClick);
            }
            
            await ToStartRoom();
        }
        
        private async UniTask Select(LobbyCameraType type)
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

        private List<LobbyCameraType> GetTypes(LobbyCameraType type)
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
            await ToRoom(DefaultRoomType);
        }
        
        private async UniTask ToRoom(LobbyCameraType type)
        {
            if(_isAnimate)
                return;
            await Select(type);
        }

        private void OnRoomClick(List<LobbyCameraType> types)
        {
            ToRoom(types[0]).Forget();
        }

        private void OnPlaceClick(LobbyCameraType type)
        {
            ToRoom(type).Forget();
        }
    }
}