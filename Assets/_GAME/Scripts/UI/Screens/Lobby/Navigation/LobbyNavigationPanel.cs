using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Lobby;
using _GAME.Scripts.UI.Screens.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby.Navigation
{
    // Lobby navigation by direct buttons (replaces LobbyBottomPanel): each button leads to a place and opens its window,
    // the window is closed when the player goes to another place
    public class LobbyNavigationPanel : LobbyPanel
    {
        [SerializeField] private List<LobbyNavigationButton> _buttons = new();

        private LobbyRoomsController _roomsController;
        private UIManager _uiManager;
        private LobbyNavigationButton _selected;
        private LobbyWindowType _openedWindow = LobbyWindowType.None;
        private bool _isMoving;

        // the lobby tutorial hides / shows the panel by this name
        private void Reset()
        {
            _panelName = "Bottom";
        }

        public async UniTask Initialize(LobbyRoomsController roomsController)
        {
            _roomsController = roomsController;
            _uiManager = Core.Get<UIManager>();
            _holderTransform.anchoredPosition = _hidePosition;
            foreach (var button in _buttons)
            {
                button.Setup(OnButtonClick);
            }
            EventBus.Subscribe<ToLobbyPlaceEvent>(OnToLobbyPlace, EventBus.EventRegion.LOBBY);
            Show().Forget();
            await GoTo(GetButton(_roomsController.StartRoom), _roomsController.StartRoom, true);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<ToLobbyPlaceEvent>(OnToLobbyPlace, EventBus.EventRegion.LOBBY);
            base.OnDestroy();
        }

        private void OnButtonClick(LobbyNavigationButton button)
        {
            GoTo(button, button.Place, false).Forget();
        }

        // external moves (tutorial etc.): the window of the first button of that place is opened
        private void OnToLobbyPlace(ToLobbyPlaceEvent placeEvent)
        {
            GoTo(GetButton(placeEvent.Type), placeEvent.Type, true).Forget();
        }

        private async UniTask GoTo(LobbyNavigationButton button, LobbyPlaceType place, bool awaitMoving)
        {
            if (_isMoving)
            {
                if (!awaitMoving)
                    return;
                await UniTask.WaitWhile(() => _isMoving);
            }

            _isMoving = true;
            try
            {
                var window = button != null ? button.Window : LobbyWindowType.None;
                if (place != _roomsController.CurrentRoomType || window != _openedWindow)
                    CloseOpenedWindow();

                _selected = button;
                foreach (var b in _buttons)
                    b.SetSelected(b == _selected);

                await _roomsController.ToRoom(place);

                // the move could be ignored (rooms controller busy) - open only at the target place
                if (_roomsController.CurrentRoomType == place)
                    OpenWindow(window);
            }
            finally
            {
                _isMoving = false;
            }
        }

        private LobbyNavigationButton GetButton(LobbyPlaceType place)
        {
            return _buttons.FirstOrDefault(b => b.Place == place);
        }

        // also reopens the window if the player closed it by hand
        private void OpenWindow(LobbyWindowType window)
        {
            switch (window)
            {
                case LobbyWindowType.Inventory: _uiManager.OpenWindow<InventoryWindow>(); break;
                case LobbyWindowType.Shop: _uiManager.OpenWindow<StoreWindow>(); break;
                case LobbyWindowType.Store: _uiManager.OpenWindow<ChestStoreWindow>(); break;
                case LobbyWindowType.Upgrade: _uiManager.OpenWindow<UpgradeWindow>(); break;
            }
            _openedWindow = window;
        }

        private void CloseOpenedWindow()
        {
            switch (_openedWindow)
            {
                case LobbyWindowType.Inventory: _uiManager.CloseWindow<InventoryWindow>(); break;
                case LobbyWindowType.Shop: _uiManager.CloseWindow<StoreWindow>(); break;
                case LobbyWindowType.Store: _uiManager.CloseWindow<ChestStoreWindow>(); break;
                case LobbyWindowType.Upgrade: _uiManager.CloseWindow<UpgradeWindow>(); break;
            }
            _openedWindow = LobbyWindowType.None;
        }
    }
}
