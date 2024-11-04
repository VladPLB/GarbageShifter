using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace _GAME.Scripts.Battle.Player
{
    public class EnemyMover: MonoBehaviour
    {
        [SerializeField] private float _stoppingDistance = .2f;
        [SerializeField] private Transform _model;
        private CharacterController _characterController;

        private bool _isActive = false;

        private EnemyBounds _enemyBounds;
        private List<Vector3> _path;
        private Coroutine _pathMoveCoroutine;
        private int _currentPathIndex = 0;

        private Transform _target;
        private Vector3 _targetPosition;
        private float _attackDistance;

        private float _speed;
        private bool _isStopped = true;
        private Vector3 _forward;

        public bool AttackedDistance => Vector3.Distance(transform.position, _target.position) <= _attackDistance;

        public event Action OnMoveCompleted;
        public event Action<float> OnMoveSpeed;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void Setup(List<Vector3> path, Transform target, EnemyBounds enemyBounds, float moveSpeed, float attackDistance)
        {
            _speed = moveSpeed;
            _attackDistance = attackDistance;
            _path = path;
            _target = target;
            _enemyBounds = enemyBounds;
            _forward = _target.transform.position - transform.position;
        }

        public void Play()
        {
            _isActive = true;
            _pathMoveCoroutine = StartCoroutine(RunPath());
        }

        private IEnumerator RunPath()
        {
            int currentIndex = 0;
            while (currentIndex <= _path.Count - 1)
            {
                var nextPoint = _path[currentIndex];
                if (currentIndex == 0)
                {
                    var targetRotation = currentIndex + 1 < _path.Count - 1
                        ? Quaternion.LookRotation((_path[1] - nextPoint).normalized)
                        : _model.rotation;
                    TeleportTo(nextPoint, targetRotation);
                }
                else
                {
                    MoveTo(nextPoint);
                }

                currentIndex++;


                yield return null;
                while (!_isStopped)
                {
                    yield return null;
                }
                if(!_isActive)
                    yield break;
            }
            Stop();
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = _targetPosition = position;
            _model.rotation  = rotation;
            _isStopped = true;
        }

        public void MoveTo(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            _isStopped = false;
        }

        public void Stop()
        {
            OnMoveSpeed?.Invoke(0);
            OnMoveCompleted?.Invoke();
            Deactivate();
        }

        private void TryClearPathCoroutine()
        {
            if (_pathMoveCoroutine != null)
            {
                StopCoroutine(_pathMoveCoroutine);
                _pathMoveCoroutine = null;
            }
        }

        public void Deactivate()
        {
            _isActive = false;
            _isStopped = true;
            OnMoveCompleted = null;
            OnMoveSpeed = null;
            TryClearPathCoroutine();
        }

        private void Update()
        {
            if(!_isActive)
                return;
            _forward = Vector3.zero;
            Vector3 move = Vector3.zero;
            Vector3 previewPosition = transform.position;
            if (!_isStopped)
            {
                _forward = (_targetPosition - transform.position).normalized;
                move = _forward * _speed * Time.deltaTime;
                _characterController.Move(move);
                if (DestinationReached())
                {
                    _isStopped = true;
                }
            }
            else
            {
                _forward = (_target.transform.position - transform.position);
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward), .1f);
            
            _characterController.transform.position = _enemyBounds.CorrectPositionWithBounds(transform.position);
            var moveDelta = (transform.position - previewPosition).magnitude;
            OnMoveSpeed?.Invoke(move == Vector3.zero? 0: Mathf.Clamp01(moveDelta/move.magnitude));
        }
        
        private bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _stoppingDistance || AttackedDistance;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, _targetPosition);
            Gizmos.DrawRay(_targetPosition, Vector3.up);
            Gizmos.color = Color.blue;
            if(_path!=null)
            {
                for (int i = 1; i < _path.Count; i++)
                {
                    Gizmos.DrawLine(_path[i] + Vector3.up * .5f, _path[i - 1] + Vector3.up * .5f);
                }
            }
        }
    }
}