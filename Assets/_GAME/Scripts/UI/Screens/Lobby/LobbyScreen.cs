using _GAME.Scripts.Common;
using _GAME.Scripts.Lobby;
using _GAME.Scripts.UI.Screens.Communications;
using _GAME.Scripts.UI.Screens.Lobby.Currency;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyScreen : UIWindow
    {
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
            await _bottomPanel.Initialize(_roomsController);
        }
    }
}