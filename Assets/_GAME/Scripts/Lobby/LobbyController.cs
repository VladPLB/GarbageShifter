using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Map;
using _GAME.Scripts.Save;
using _GAME.Scripts.Tutorial;
using _GAME.Scripts.UI.Screens.Communications;
using _GAME.Scripts.UI.Screens.Lobby;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyRoomsController _roomsController;
        [SerializeField] private MapController _mapController;
        [SerializeField] private List<TutorialController> _tutorialControllers;

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
            foreach (var tutorialController in _tutorialControllers)
            {
                tutorialController.Initialize();
            }
            _roomsController.Initialize(_mapController);
            
            var (zoneIndex, locationIndex, levelIndex) = _mapManager.GetInfo(_progressData.Level);
            LevelZoneData zoneData = _mapManager.GetZone(zoneIndex);
            zoneData.Setup();
            EventBus.Push(new KeyEvent("RoomsLoaded"), EventBus.EventRegion.LOBBY);
            EventBus.Push(new MusicPlayEvent(MusicTrack.Menu), EventBus.EventRegion.GLOBAL);
            await UniTask.DelayFrame(1);
            await ShowLobbyScreen();
            _mapController.Initialize(zoneData, locationIndex, levelIndex);
            EventBus.Push(new KeyEvent("SceneLoaded"), EventBus.EventRegion.GLOBAL);
        }

        private async UniTask ShowLobbyScreen()
        {
            var lobbyScreen = _uiManager.OpenWindow<LobbyScreen>();
            await lobbyScreen.Initialize(_roomsController);
            EventBus.Subscribe<OpenDialogScreenEvent>(OnOpenDialogScreen, EventBus.EventRegion.LOBBY);
        }

        private void OnOpenDialogScreen(OpenDialogScreenEvent e)
        {
            if (e.IsOpen)
            {
                var screen =_uiManager.OpenWindow<UIDialog>();
                screen.SetPosition(e.PositionType);
            }
            else
            {
                _uiManager.CloseWindow<UIDialog>();
            }
        }

        private void OnDisable()
        {
            EventBus.Push(new AmbientStopEvent(AmbientType.All), EventBus.EventRegion.GLOBAL);
            _uiManager.ClearWindowPool();
            _uiManager.CloseAllWindows();
            EventBus.Unsubscribe<OpenDialogScreenEvent>(OnOpenDialogScreen, EventBus.EventRegion.LOBBY);
        }
    }
}