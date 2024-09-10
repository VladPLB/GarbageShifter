using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using Cinemachine;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private List<VCameraByType> _cameras;

        public void SetCamera(GameCameraType type)
        {
            _cameras.ForEach(c=>c.VCamera.enabled = c.Type == type);
        }

        [Serializable]
        public struct VCameraByType
        {
            public GameCameraType Type;
            public CinemachineVirtualCamera VCamera;
        }
    }
}