using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class LobbyCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _lobbyCamera;
        [SerializeField] private List<VCameraByType> _cameras;

        public void SetCamera(LobbyCameraType type)
        {
            _lobbyCamera.enabled = type != LobbyCameraType.Map;
            _cameras.ForEach(c=>c.VCamera.enabled = c.Type == type);
        }

        [Serializable]
        public struct VCameraByType
        {
            public LobbyCameraType Type;
            public CinemachineVirtualCamera VCamera;
        }

        private void OnEnable()
        {
            SetCamera(LobbyCameraType.Transition);
        }
    }
}