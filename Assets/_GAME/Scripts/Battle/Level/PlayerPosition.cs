using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class PlayerPosition: MonoBehaviour
    {
        private PlayerPositionType _type;
        public PlayerPositionType Type => _type;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public void Setup(PlayerPositionType type)
        {
            _type = type;
        }
    }
}