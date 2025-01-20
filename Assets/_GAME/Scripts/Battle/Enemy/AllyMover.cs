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
    public class AllyMover: MonoBehaviour
    {
        
        [SerializeField] protected Transform _model;
        private CharacterController _characterController;
        protected float _stoppingDistance = .3f;
        protected bool _isActive = false;

        protected Transform _target;
        protected Vector3 _targetPosition;

        protected float _speed;
        protected bool _isStopped = true;
        protected Vector3 _forward;

        private Func<Transform> _getTargetFunc;
        public Action OnMoveCompleted;
        public Action<float> OnMoveSpeed;

        public bool IsActive => _isActive;
        public bool IsStopped => _isStopped;

        public Vector3 Forward => _model.forward;

        protected virtual void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public virtual void Setup(Vector3 spawnPoint, Vector3 targetPoint, float moveSpeed, Func<Transform> getTargetFunc)
        {
            _speed = moveSpeed;
            _target = null;
            _targetPosition = targetPoint;
            _forward = (targetPoint - spawnPoint).normalized;
            _getTargetFunc = getTargetFunc;
            TeleportTo(spawnPoint, Quaternion.Euler(_forward));
        }

        public void TargetChange(Transform target)
        {
            _target = target;
        }

        public void Play()
        {
            MoveTo(_targetPosition);
            _isActive = true;
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
            _isStopped = true;
            OnMoveSpeed?.Invoke(0);
            OnMoveCompleted?.Invoke();
        }

        public void Deactivate()
        {
            _isActive = false;
            _isStopped = true;
            OnMoveCompleted = null;
            OnMoveSpeed = null;
        }

        protected virtual void Update()
        {
            if(!_isActive)
                return;
            _forward = _model.forward;
            Vector3 move = Vector3.zero;
            Vector3 previewPosition = transform.position;
            if (!_isStopped)
            {
                _forward = (_targetPosition - transform.position).normalized;
                move = _forward * _speed * Time.deltaTime;
                _characterController.Move(move);
                if (DestinationReached())
                {
                    Stop();
                }
            }
            else
            {
                _target = _getTargetFunc?.Invoke();
                if(_target != null)
                {
                    _forward = transform.position.ZeroHeightRotation(_target.transform.position);
                }
            }
            _model.rotation = Quaternion.Lerp(_model.rotation, Quaternion.LookRotation(_forward), .1f);
            var moveDelta = (transform.position - previewPosition).magnitude;
            OnMoveSpeed?.Invoke(move == Vector3.zero? 0: Mathf.Clamp01(moveDelta/move.magnitude));
        }
        
        protected bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _stoppingDistance;
    }
}