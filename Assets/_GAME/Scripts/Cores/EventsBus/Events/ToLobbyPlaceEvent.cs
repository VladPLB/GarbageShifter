using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class ToLobbyPlaceEvent: IEvent
    {
        public LobbyPlaceType Type;
        
        public ToLobbyPlaceEvent(LobbyPlaceType type) => Type = type;
    }
}