using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class OnLobbyPlaceEvent: IEvent
    {
        public LobbyPlaceType Type;
        
        public OnLobbyPlaceEvent(LobbyPlaceType type) => Type = type;
    }
}