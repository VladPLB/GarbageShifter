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
        [SerializeField] private List<LobbyCameraType> _types;

        public List<LobbyCameraType> Types => _types;
        
        public bool Has(LobbyCameraType type) => _types.Contains(type);

        public void SetRoomActive(bool isActive) => _room?.SetActive(isActive);
    }
}