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
        
        [SerializeField] protected Transform _model;
        private CharacterController _characterController;
        protected float _stoppingDistance = .3f;
        protected bool _isActive = false;

        protected EnemyBounds _enemyBounds;
        protected List<Vector3> _path;
        protected Coroutine _pathMoveCoroutine;

        protected Transform _target;
        protected Vector3 _targetPosition;
        protected float _attackDistance;

        protected float _speed;
        protected bool _isStopped = true;
        protected Vector3 _forward;
        protected bool _isJumpToPlayer = false;
        
        protected Action _jumpToPlayerCallback;
        public Action OnMoveCompleted;
        public Action<float> OnMoveSpeed;
        
        public bool IsAttackedDistance => Vector3.Distance(transform.position, _target.position) <= _attackDistance;

        protected virtual void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public virtual void Setup(List<Vector3> path, Transform target, EnemyBounds enemyBounds, float moveSpeed, float attackDistance)
        {
            _speed = moveSpeed;
            _attackDistance = attackDistance;
            _path = path;
            _target = target;
            _enemyBounds = enemyBounds;
            _forward = _target.transform.position - transform.position;
            _isJumpToPlayer = false;
            var targetRotation = _path.Count>1
                ? Quaternion.LookRotation((_path[1] - _path[0]).normalized)
                : _model.rotation;
            TeleportTo(_path[0], targetRotation);
        }

        public void Play()
        {
            _pathMoveCoroutine = StartCoroutine(RunPath());
            _isActive = true;
        }

        protected IEnumerator RunPath()
        {
            int currentIndex = 1;
            while (currentIndex <= _path.Count - 1)
            {
                var nextPoint = _path[currentIndex];
                MoveTo(nextPoint);
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

        public virtual void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = _targetPosition = position;
            _model.rotation  = rotation;
            _isStopped = true;
        }

        public virtual void MoveTo(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            _isStopped = false;
        }

        public virtual void Stop()
        {
            OnMoveSpeed?.Invoke(0);
            OnMoveCompleted?.Invoke();
            Deactivate();
        }

        protected void TryClearPathCoroutine()
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

        protected virtual void Update()
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
                    if (IsAttackedDistance)
                    {
                        TryClearPathCoroutine();
                    }
                    _isStopped = true;
                }
            }
            else
            {
                _forward = (_target.transform.position - transform.position);
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward.normalized), .1f);
            
            if(_enemyBounds.TryCorrectPositionWithBounds(transform.position, out var correctedPosition))
            {
                _characterController.transform.position = correctedPosition;
            }
            var moveDelta = (transform.position - previewPosition).magnitude;
            OnMoveSpeed?.Invoke(move == Vector3.zero? 0: Mathf.Clamp01(moveDelta/move.magnitude));
        }
        
        protected bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _stoppingDistance || IsAttackedDistance;

        public virtual void JumpToPlayer( Action callback)
        {
            _jumpToPlayerCallback = callback;
            _jumpToPlayerCallback?.Invoke();
            _jumpToPlayerCallback = null;
        }
        
        protected virtual void OnDrawGizmosSelected()
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