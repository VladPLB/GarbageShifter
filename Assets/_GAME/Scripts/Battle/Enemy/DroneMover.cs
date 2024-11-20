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
        [SerializeField] private LayerMask _borderLayerMask;
        private Rigidbody _rigidBody;

        protected override void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }
        
        public override void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = _targetPosition = position + Vector3.up * _borderDistance;
            _model.rotation  = rotation;
            _isStopped = true;
        }

        public override void MoveTo(Vector3 targetPosition)
        {
            _targetPosition = targetPosition  + Vector3.up * _borderDistance;
            _isStopped = false;
        }

        private bool TryCorrectPositionByBorders(out Vector3 correctedPosition)
        {
            var pos = transform.position;
            var offset = Vector3.zero;
            for(float x = -1f;x<1f;x+=.5f)
            {
                for (float y = -1f; y < 1f; y += .5f)
                {
                    for (float z = -1f; z < 1f; z += .5f)
                    {
                        var dir = new Vector3(x, y, z);
                        if (Physics.Raycast(new Ray(pos, dir), out var hit, _borderDistance, _borderLayerMask))
                        {
                            offset += dir * -1f * (_borderDistance - hit.distance);
                            Debug.DrawLine(pos, hit.point, Color.red);
                        }
                        else
                        {
                            Debug.DrawRay(pos, dir, Color.green);
                        }
                    }
                }
            }

            correctedPosition = pos + offset;

            return offset != Vector3.zero;
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
            Vector3 previewPosition = transform.position;
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
                _forward = (_targetPosition - transform.position).normalized;
                move = _forward * _speed * 30f * Time.fixedDeltaTime;
                _rigidBody.velocity = move;
                if (DestinationReached())
                {
                    if (IsAttackedDistance)
                    {
                        Stop();
                    }
                    _isStopped = true;
                }
            }
            else
            {
                _rigidBody.velocity = Vector3.zero;
                _forward = (_target.transform.position - transform.position);
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward), .1f);
            Vector3 correctedPosition;
            if (TryCorrectPositionByBorders(out correctedPosition))
            {
                _rigidBody.position = Vector3.Lerp(_rigidBody.position, correctedPosition, .2f);
            }
            if(_enemyBounds.TryCorrectPositionWithBounds(transform.position, out correctedPosition))
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