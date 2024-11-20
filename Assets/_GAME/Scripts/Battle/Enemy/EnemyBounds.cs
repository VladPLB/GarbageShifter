using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class EnemyBounds
    {
        private const float MAX_FRONT_ANGLE = 140f; 
        private const float BOUNDARY_DISTANCE = 5f;
        
        private Vector3 _playerPosition;
        private Vector3 _playerForward;
        private Vector3 _playerLeft;
        private Vector3 _playerRight;
        private Vector3 _leftBoundaryDirection;
        private Vector3 _rightBoundaryDirection;

        private float _leftDot;
        private float _rightDot;

        public EnemyBounds(Vector3 playerPosition, Vector3 playerForward)
        {
            _playerPosition = playerPosition;
            _playerForward = playerForward;
            
            Quaternion leftRotation = Quaternion.Euler(0, -MAX_FRONT_ANGLE / 2f, 0);
            Quaternion rightRotation = Quaternion.Euler(0, MAX_FRONT_ANGLE / 2f, 0);
            _leftBoundaryDirection= leftRotation * _playerForward;
            _playerLeft = Quaternion.Euler(0, -90, 0) * _playerForward;
            _leftDot = Vector3.Dot(_playerLeft, _leftBoundaryDirection);
            _rightBoundaryDirection = rightRotation * _playerForward;
            _playerRight = Quaternion.Euler(0, 90, 0) * _playerForward;
            _rightDot = Vector3.Dot(_playerRight, _rightBoundaryDirection);
        }
        
        public bool TryCorrectPositionWithBounds(Vector3 unitPosition, out Vector3 correctedPosition)
        {
            var unitY = unitPosition.y;
            var dots = GetDots(unitPosition);
            var left = dots.Item1;
            var right = dots.Item2;

            bool isLeftBounds = (left > 0 && _leftDot < left);
            bool isRightBounds = (right > 0 && _rightDot < right);
            
            if ( isLeftBounds || isRightBounds)
            {
                var dir = isLeftBounds ? _leftBoundaryDirection : _rightBoundaryDirection;
                var intersectPoint = GetIntersectionPoint(_playerPosition, unitPosition, dir, _playerForward);
                unitPosition = intersectPoint;
            }
            
            unitPosition.y = unitY;
            correctedPosition = unitPosition;
            return isLeftBounds || isRightBounds;
        }
        
        private Vector3 GetIntersectionPoint(Vector3 point1, Vector3 point2, Vector3 dir1, Vector3 dir2)
        {
            Vector2 P1 = new Vector2(point1.x, point1.z);
            Vector2 P2 = new Vector2(point2.x, point2.z);
            
            Vector2 d1 = new Vector2(dir1.x, dir1.z);
            Vector2 d2 = new Vector2(dir2.x, dir2.z);
            
            Vector2 P2_P1 = P2 - P1;
            float determinant = d1.x * d2.y - d1.y * d2.x;

            if (Mathf.Abs(determinant) < Mathf.Epsilon)
            {
                return point1;
            }

            float t = (P2_P1.x * d2.y - P2_P1.y * d2.x) / determinant;
            Vector2 intersectionPoint = P1 + t * d1;
            return new Vector3(intersectionPoint.x, 0, intersectionPoint.y);
        }

        private (float, float) GetDots(Vector3 position)
        {
            Vector3 toUnit = (position - _playerPosition ).normalized;
            return (Vector3.Dot(_playerLeft, toUnit) , Vector3.Dot(_playerRight, toUnit));
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_playerPosition,_playerPosition + _leftBoundaryDirection * BOUNDARY_DISTANCE);
            Gizmos.DrawLine(_playerPosition, _playerPosition + _rightBoundaryDirection * BOUNDARY_DISTANCE);

        }
    }
}