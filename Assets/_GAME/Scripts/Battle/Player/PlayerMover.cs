using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace _GAME.Scripts.Battle.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerMover: MonoBehaviour
    {
        private const float MOVE_SPEED = 5f;
        private const float FLY_SPEED = 10f;
        private const float FLY_SPEED_LERP_TIME = 1f;
        private const float FLY_DELAY = 1.6f;
        
        private NavMeshAgent _navMeshAgent;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isEnableFly = true;
        private float _delayToFly = 0;
        private bool _isFly = false;
        
        [SerializeField]
        private SoundType _footstepSound;

        [SerializeField] private Transform _leftFoot;
        [SerializeField] private Transform _rightFoot;
        
        bool _isLeftFoot = false;
        private float _footStepDelay = .1f;
        
        [SerializeField]
        private AmbientType _flySound;

        public event Action OnMoveCompleted;
        public event Action OnFly;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            _navMeshAgent.Warp(position);
            _navMeshAgent.isStopped = false;
            _navMeshAgent.ResetPath();
            _targetRotation = rotation;
            _targetPosition = position;
            transform.rotation = rotation;
        }

        public void MoveTo(Vector3 targetPosition, Quaternion targetRotation, bool isEnableFly)
        {
            SetSpeed(MOVE_SPEED);
            _targetPosition = targetPosition;
            _navMeshAgent.SetDestination(targetPosition);
            _navMeshAgent.isStopped = false;
            _targetRotation = targetRotation;
            _isEnableFly = isEnableFly;
            _isFly = false;
            _delayToFly = FLY_DELAY;
        }

        private void SetSpeed(float speed)
        {
            _navMeshAgent.speed = speed;
        }

        public void Stop()
        {
            _navMeshAgent.isStopped = true;
            OnMoveCompleted?.Invoke();
            OnMoveCompleted = null;
            AudioManager.Stop(_flySound, false);
            _footStepDelay = .08f;
        }

        private void Update()
        {
            if (!_navMeshAgent.isOnNavMesh)
            {
                return;
            }
            
            if (!_navMeshAgent.isStopped)
            {
                if (DestinationReached())
                {
                    Stop();
                    return;
                }

                if (!_isFly)
                {
                    if(_isEnableFly)
                    {
                        _delayToFly -= Time.deltaTime;
                        if (_delayToFly <= 0)
                        {
                            Fly();
                        }
                    }

                    if (_footStepDelay < 0f)
                    {
                        if (Physics.Raycast(_isLeftFoot ? _leftFoot.position : _rightFoot.position, Vector3.down,
                                out RaycastHit hit, .1f))
                        {
                            AudioManager.Play(_footstepSound,
                                (_isLeftFoot ? _leftFoot.position : _rightFoot.position) + Vector3.up);
                            _isLeftFoot = !_isLeftFoot;
                            _footStepDelay = .08f;
                        }
                    }
                    else
                    {
                        _footStepDelay -= Time.deltaTime;
                    }
                }
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, .05f);
                transform.position = Vector3.Lerp(transform.position, _targetPosition, .05f);
            }
        }

        private void Fly()
        {
            _isFly = true;
            _footStepDelay = .08f;
            OnFly?.Invoke();
            OnFly = null;
            DOTween.To(() => _navMeshAgent.speed, SetSpeed, FLY_SPEED, FLY_SPEED_LERP_TIME).SetEase(Ease.OutSine);
            AudioManager.Play(_flySound);
        }
        
        private bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _navMeshAgent.stoppingDistance;
    }
}