using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyRoom : MonoBehaviour
    {
        [SerializeField] private GameObject _room;
        [SerializeField] private List<LobbyPlaceType> _types;

        public List<LobbyPlaceType> Types => _types;
        
        public bool Has(LobbyPlaceType type) => _types.Contains(type);

        public void SetRoomActive(bool isActive) => _room?.SetActive(isActive);
    }
}