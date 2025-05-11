using System;
using _GAME.Scripts.UI.Screens.Lobby;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyRoomsController _roomsController;
        
        private UIManager _uiManager;
        private void OnEnable()
        {
            _uiManager = Core.Get<UIManager>();
            _roomsController.Initialize();
            
            ShowLobbyScreen();
        }

        private void ShowLobbyScreen()
        {
            var lobbyScreen = _uiManager.OpenWindow<LobbyScreen>();
            lobbyScreen.Initialize(_roomsController);
        }

        private void OnDisable()
        {
            _uiManager.ClearWindowPool();
        }
    }
}