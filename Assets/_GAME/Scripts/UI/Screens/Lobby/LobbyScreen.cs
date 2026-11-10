using _GAME.Scripts.Common;
using _GAME.Scripts.Lobby;
using _GAME.Scripts.UI.Screens.Communications;
using _GAME.Scripts.UI.Screens.Lobby.Currency;
using _GAME.Scripts.UI.Screens.Lobby.Navigation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyScreen : UIWindow
    {
        [Tooltip("Direct place buttons; when set, the old bottom panel is not used")]
        [SerializeField] private LobbyNavigationPanel _navigationPanel;
        [SerializeField] private LobbyBottomPanel _bottomPanel;
        [SerializeField] private CurrencyPanel _currencyPanel;

        private LobbyRoomsController _roomsController;
        
        public async UniTask Initialize(LobbyRoomsController roomsController)
        {
            _roomsController = roomsController;
            InitReferences();
            await InitBehaviours();
        }
        
        private void InitReferences()
        {
            
        }

        private async UniTask InitBehaviours()
        {
            _currencyPanel.Initialize();
            if (_navigationPanel != null)
            {
                if (_bottomPanel != null)
                    _bottomPanel.gameObject.SetActive(false);
                await _navigationPanel.Initialize(_roomsController);
            }
            else
            {
                await _bottomPanel.Initialize(_roomsController);
            }
        }
    }
}