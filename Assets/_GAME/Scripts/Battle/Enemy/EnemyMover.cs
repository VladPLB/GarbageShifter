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
        [Header("Avoidance")] 
        [SerializeField] protected LayerMask _avoidanceLayerMask;
        [SerializeField] protected float _avoidanceDetectRadius = 4.5f;
        [SerializeField] protected float _avoidanceStrength = 5f;
        
        private CharacterController _characterController;
        protected float _stoppingDistance = .3f;
        protected bool _isActive = false;

        protected EnemyBounds _enemyBounds;
        protected List<Vector3> _path;
        protected Coroutine _pathMoveCoroutine;

        protected Transform _target;
        protected Vector3 _targetPosition;
        protected float _attackDistance;
        protected float _stopDistance;

        protected float _speed;
        protected bool _isStopped = true;
        protected Vector3 _forward;
        protected bool _isJumpToPlayer = false;
        
        protected float _avoidanceDetectSqrRadius;
        protected Collider[] _avoidanceDetectedColliders;
        
        protected Action _jumpToPlayerCallback;
        public Action OnMoveCompleted;
        public Action<float> OnMoveSpeed;
        
        public bool IsStoppingDistance => Vector3.Distance(transform.position, _target.position) <= _stopDistance;
        public bool IsAttackDistance => Vector3.Distance(transform.position, _target.position) <= _stopDistance;

        protected virtual void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public virtual void Setup(List<Vector3> path, Transform target, EnemyBounds enemyBounds, float moveSpeed, float stoppingDistance, float attackDistance)
        {
            _speed = moveSpeed;
            _stopDistance = stoppingDistance;
            _attackDistance = attackDistance;
            _path = path;
            _target = target;
            _enemyBounds = enemyBounds;
            _forward = _target.transform.position - transform.position;
            _isJumpToPlayer = false;
            if(_characterController!=null)
                _characterController.enabled = true;
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
            if(NavMesh.SamplePosition(position, out var hit, 100, NavMesh.AllAreas))
                position = hit.position;
            transform.position = _targetPosition = position;
            _model.rotation  = rotation;
            _isStopped = true;
        }

        public virtual void MoveTo(Vector3 targetPosition)
        {
            if (NavMesh.SamplePosition(targetPosition, out var hit, 100, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
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
            if(_characterController!=null)
                _characterController.enabled = false;
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
                move = _forward * _speed;
                if (TryGetAvoidanceForce(out var avoidanceForce))
                {
                    move += avoidanceForce;
                }
                move *=Time.deltaTime;
                if(_characterController!=null)
                    _characterController.Move(move);
                if (DestinationReached())
                {
                    if (IsStoppingDistance)
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
                if(_characterController!=null)
                    _characterController.transform.position = correctedPosition;
            }
            var moveDelta = (transform.position - previewPosition).magnitude;
            OnMoveSpeed?.Invoke(move == Vector3.zero? 0: Mathf.Clamp01(moveDelta/move.magnitude));
        }
        
        protected bool TryGetAvoidanceForce(out Vector3 force)
        {
            var selfPosition = transform.position;
            var collidersCount = Physics.OverlapSphereNonAlloc(selfPosition, _avoidanceDetectRadius, _avoidanceDetectedColliders,
                _avoidanceLayerMask);
            Vector3 avoidanceForce = Vector3.zero;
            int count = 0;

            for (int i = 0; i < collidersCount; i++)
            {
                Collider hit = _avoidanceDetectedColliders[i];
                if (hit.transform == transform) continue;

                Vector3 hitTransformPosition = hit.transform.position;
                Vector3 dir = selfPosition - hitTransformPosition;
                dir.y = 0;
                float sqrDistance = selfPosition.ZeroHeightSqrDistanceTo(hitTransformPosition);

                Vector3 repelDir = dir.normalized;

                float strength = Mathf.Lerp(_avoidanceStrength, 0, sqrDistance / _avoidanceDetectSqrRadius);
                strength *= strength;

                avoidanceForce += repelDir * strength;
                count++;
            }

            force = count > 0 ? avoidanceForce / count : Vector3.zero;
            return force != Vector3.zero;
        }
        
        protected bool DestinationReached() => Vector3.Distance(transform.position, _targetPosition) <= _stoppingDistance || IsStoppingDistance;

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