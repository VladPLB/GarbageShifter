using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI.Screens.Lobby;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyRoomsController _roomsController;
        [SerializeField] private SkyboxSettings _skyboxSettings;
        
        private UIManager _uiManager;
        private async void OnEnable()
        {
            _uiManager = Core.Get<UIManager>();
            _roomsController.Initialize();
            
            await ShowLobbyScreen();
            _skyboxSettings.Setup();
            EventBus.Push(new KeyEvent("SceneLoaded"), EventBus.EventRegion.GLOBAL);
        }

        private async UniTask ShowLobbyScreen()
        {
            var lobbyScreen = _uiManager.OpenWindow<LobbyScreen>();
            await lobbyScreen.Initialize(_roomsController);
        }

        private void OnDisable()
        {
            _uiManager.ClearWindowPool();
        }
    }
}