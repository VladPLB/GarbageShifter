using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class SetLobbyStartPlaceEvent: IEvent
    {
        public LobbyPlaceType Type;
        
        public SetLobbyStartPlaceEvent(LobbyPlaceType type) => Type = type;
    }
}