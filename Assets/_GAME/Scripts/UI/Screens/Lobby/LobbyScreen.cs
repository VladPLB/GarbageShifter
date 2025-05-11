using _GAME.Scripts.Common;
using _GAME.Scripts.Lobby;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyScreen : UIWindow
    {
        [SerializeField] private LobbyBottomPanel _bottomPanel;
        
        private LobbyRoomsController _roomsController;

        public void Initialize(LobbyRoomsController roomsController)
        {
            _roomsController = roomsController;
            InitReferences();
            InitBehaviours();
        }
        
        private void InitReferences()
        {
            
        }

        private void InitBehaviours()
        {
            _bottomPanel.Initialize(_roomsController);
        }
    }
}