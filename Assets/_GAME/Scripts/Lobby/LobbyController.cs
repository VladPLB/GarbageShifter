using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Map;
using _GAME.Scripts.Save;
using _GAME.Scripts.UI.Screens.Lobby;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyRoomsController _roomsController;
        [SerializeField] private MapController _mapController;

        private UIManager _uiManager;
        private MapManager _mapManager;
        private SaveManager _saveManager;

        private ProgressData _progressData;
        
        private async void OnEnable()
        {
            _uiManager = Core.Get<UIManager>();
            _mapManager = Core.Get<MapManager>();
            _saveManager = Core.Get<SaveManager>();

            _progressData = _saveManager.GetData<ProgressData>();
            
            _roomsController.Initialize(_mapController);

            var (zoneIndex, locationIndex, levelIndex) = _mapManager.GetInfo(_progressData.Level);
            LevelZoneData zoneData = _mapManager.GetZone(zoneIndex);
            zoneData.Setup();
            
            await ShowLobbyScreen();
            _mapController.Initialize(zoneData, locationIndex, levelIndex);
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