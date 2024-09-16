using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace _GAME.Scripts.Battle.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyMover: MonoBehaviour
    {
        [SerializeField] private float _stoppingDistance = .2f;
        [SerializeField] private Transform _model;
        private Rigidbody _rigidbody;

        private EnemyBounds _enemyBounds;
        private List<Vector3> _path;
        private Coroutine _pathMoveCoroutine;

        private Transform _target;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _attackDistance;
        
        private float _speed;
        private bool _isStopped = true;
        private Vector3 _forward;

        public event Action OnMoveCompleted;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Setup(List<Vector3> path, Transform target, EnemyBounds enemyBounds, float moveSpeed, float attackDistance)
        {
            _speed = moveSpeed;
            _attackDistance = attackDistance;
            _path = path;
            _target = target;
            _enemyBounds = enemyBounds;
            _rigidbody.isKinematic = true;
            _forward = _target.transform.position - transform.position;
        }

        public void Play()
        {
            _pathMoveCoroutine = StartCoroutine(RunPath());
        }

        private IEnumerator RunPath()
        {
            int currentIndex = 0;
            while (currentIndex <= _path.Count - 1)
            {
                var targetRotation = currentIndex + 1 < _path.Count - 1
                    ? Quaternion.LookRotation((_path[currentIndex + 1] - _path[currentIndex]).normalized)
                    : _model.rotation;
                var nextPoint = _path[currentIndex];
                if (currentIndex == 0)
                {
                    TeleportTo(nextPoint, targetRotation);
                }
                else
                {
                    MoveTo(nextPoint, targetRotation);
                }

                currentIndex++;


                yield return null;
                while (!_isStopped)
                {
                    yield return null;
                }
            }
            Stop();
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = _targetPosition = position;
            _model.rotation = _targetRotation = rotation;
            _rigidbody.isKinematic = false;
            _isStopped = true;
        }

        public void MoveTo(Vector3 targetPosition, Quaternion targetRotation)
        {
            _targetPosition = targetPosition;
            _isStopped = false;
            _rigidbody.isKinematic = false;
            _targetRotation = targetRotation;
        }

        public void Stop()
        {
            OnMoveCompleted?.Invoke();
            OnMoveCompleted = null;
            _isStopped = true;
            TryClearPathCoroutine();
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
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.isKinematic = true;
            _isStopped = true;
            OnMoveCompleted = null;
            TryClearPathCoroutine();
        }

        private void FixedUpdate()
        {
            if(_rigidbody.isKinematic)
                return;
            
            if (!_isStopped)
            {
                
                _forward = (_targetPosition - transform.position).normalized;
                _rigidbody.velocity = _forward * (_speed * 30f * Time.fixedDeltaTime);
                if (DestinationReached())
                {
                    _isStopped = true;
                    _rigidbody.velocity = Vector3.zero;
                }
            }
            else
            {
                _forward = _target.transform.position - transform.position;
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward), .1f);

            transform.position = _enemyBounds.CorrectPositionWithBounds(transform.position);
        }
        
        private bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _stoppingDistance;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, _targetPosition);
            Gizmos.DrawRay(_targetPosition, Vector3.up);
        }
    }
}