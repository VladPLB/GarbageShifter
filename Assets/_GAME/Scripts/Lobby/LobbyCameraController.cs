using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using Unity.Cinemachine;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _lobbyCamera;
        [SerializeField] private List<VCameraByType> _cameras;

        public void SetCamera(LobbyPlaceType type)
        {
            _lobbyCamera.enabled = type != LobbyPlaceType.Map;
            _cameras.ForEach(c=>c.VCamera.enabled = c.Type == type);
        }

        [Serializable]
        public struct VCameraByType
        {
            public LobbyPlaceType Type;
            public CinemachineCamera VCamera;
        }

        private void OnEnable()
        {
            SetCamera(LobbyPlaceType.Transition);
        }
    }
}