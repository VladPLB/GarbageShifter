using UnityEngine;

namespace _GAME.Scripts.Common
{
    public class FollowTarget:MonoBehaviour
    {
        [SerializeField] private Transform _followTarget;
        [SerializeField] private bool _followPosition = true;
        [SerializeField, Range(0f,1f)] private float _positionSmoothTime = 1f;
        [SerializeField] private bool _followRotation = true;
        [SerializeField, Range(0f,1f)] private float _rotationSmoothTime = 1f;
        

        public void SetTarget(Transform t) => _followTarget = t;

        private void LateUpdate()
        {
            if (_followTarget == null) return;

            if (_followPosition)
            {
                transform.position = _positionSmoothTime<1f? _followTarget.position: Vector3.Lerp(transform.position, _followTarget.position, _positionSmoothTime);
            }
            if (_followRotation)
            {
                transform.rotation =  _rotationSmoothTime<1f? _followTarget.rotation: Quaternion.Lerp(transform.rotation, _followTarget.rotation, _rotationSmoothTime);
            }
        }
    }

}