using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Lobby;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyRoomsButtons : MonoBehaviour
    {
        private const LobbyCameraType DefaultRoomType = LobbyCameraType.Bar_Barmen;
        
        [SerializeField] 
        private List<BottomPanelRoomButton> _roomButtons;

        [SerializeField] private RoomPlacesButtons _placesButtonsHolder;

        private LobbyRoomsController _roomsController;
        private bool _isAnimate = false;
        private List<LobbyCameraType> _types;

        private void OnEnable()
        {
            _roomsController = Core.Get<LobbyRoomsController>();
            Initialize(DefaultRoomType);
        }

        public void Initialize(LobbyCameraType type)
        {
            _placesButtonsHolder.Setup();
            foreach (var roomButton in _roomButtons)
            {
                roomButton.Setup(OnRoomClick);
                if (roomButton.Types.Contains(type))
                {
                    OnRoomClick(roomButton.Types);
                }
            }
        }
        
        private async void Select(LobbyCameraType type)
        { 
            if(_isAnimate)
                return;
            
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
        
        

        private void OnRoomClick(List<LobbyCameraType> types)
        {
            if(_isAnimate)
                return;
            _types = types;
            Select(types[0]);
        }

        private void OnPlaceClick(LobbyCameraType type)
        {
            if(_isAnimate)
                return;
            Select(type);
        }
    }
}