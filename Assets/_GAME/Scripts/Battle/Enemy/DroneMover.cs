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
    public class DroneMover: EnemyMover
    {
        [SerializeField] private float _borderDistance = 1f;
        [SerializeField] private Vector2 _heightRange = new Vector2(2f,2.5f);
        [SerializeField] private LayerMask _borderLayerMask;
        private Rigidbody _rigidBody;
        private float _height;

        protected override void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        public override void Setup(List<Vector3> path, Transform target, EnemyBounds enemyBounds, float moveSpeed, float stoppingDistance, float attackDistance)
        {
            _rigidBody ??= GetComponent<Rigidbody>();
            base.Setup(path, target, enemyBounds, moveSpeed, stoppingDistance, attackDistance);
            _height = _heightRange.GetRandom();
        }

        public override void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = _rigidBody.position = _targetPosition = position;
            _model.rotation  = rotation;
            _isStopped = true;
        }

        public override void MoveTo(Vector3 targetPosition)
        {
            _targetPosition = targetPosition  + Vector3.up * _borderDistance;
            _isStopped = false;
        }

        private bool TryCorrectPositionByHeight(out Vector3 correctedPosition)
        {
            correctedPosition = _rigidBody.position;
            if (Mathf.Abs(correctedPosition.y - _height) < .1f)
            {
                return false;
            }
            correctedPosition.y = Mathf.MoveTowards(correctedPosition.y, _height, _speed * 90f * Time.fixedDeltaTime);
            if (Physics.Raycast(new Ray(_rigidBody.position, Vector3.down), out var hitD, .5f, _borderLayerMask))
            {
                correctedPosition.y = Mathf.Max(correctedPosition.y, hitD.point.y + .1f);
            }
            if (Physics.Raycast(new Ray(_rigidBody.position, Vector3.up), out var hitU, .5f, _borderLayerMask))
            {
                correctedPosition.y = Mathf.Min(correctedPosition.y, hitU.point.y - .1f);
            }
            return true;
        }
        
        public override void Stop()
        {
            _isStopped = true;
            TryClearPathCoroutine();
        }

        protected override void Update()
        {
            
        }

        protected void FixedUpdate()
        {
            if (!_isActive)
            {
                _rigidBody.velocity = Vector3.zero;
                return;
            }
                
            _forward = Vector3.zero;
            Vector3 move = Vector3.zero;
            Vector3 previewPosition = _rigidBody.position;
            if (_isJumpToPlayer)
            {
                _forward = (_target.transform.position - transform.position);
                move = _forward * _speed * 90f * Time.fixedDeltaTime;
                _rigidBody.velocity = move;
                var selfPos = transform.position;
                var targetPos = _target.position;
                selfPos.y = targetPos.y = 0;
                if ( Vector3.Distance(selfPos, targetPos) <= _stoppingDistance)
                {
                    Stop();
                    _isJumpToPlayer = false;
                    _jumpToPlayerCallback?.Invoke();
                }
            }
            else if (!_isStopped)
            {
                if (TryCorrectPositionByHeight(out var cPos))
                {
                    _rigidBody.position = Vector3.Lerp(_rigidBody.position, cPos, .2f);
                }
                _forward = (_targetPosition - _rigidBody.position).normalized;
                move = _forward * _speed * 30f * Time.fixedDeltaTime;
                _rigidBody.velocity = move;
                if (DestinationReached())
                {
                    if (IsStoppingDistance)
                    {
                        Stop();
                    }
                    _isStopped = true;
                }
            }
            else
            {
                _rigidBody.velocity = Vector3.zero;
                _forward = ((_target.transform.position+Vector3.up) - transform.position);
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward.normalized), .1f);
            Vector3 correctedPosition;
            if(_enemyBounds.TryCorrectPositionWithBounds(_rigidBody.position, out correctedPosition))
            {
                _rigidBody.position = correctedPosition;
            }
            var moveDelta = (transform.position - previewPosition).magnitude;
            OnMoveSpeed?.Invoke(move == Vector3.zero? 0: Mathf.Clamp01(moveDelta/move.magnitude));
        }

        public override void JumpToPlayer(Action callback)
        {
            TryClearPathCoroutine();
            _jumpToPlayerCallback = callback;
            _isJumpToPlayer = true;
        }
    }
}