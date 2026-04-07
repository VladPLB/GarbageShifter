using System;
using System.Collections;
using _GAME.Scripts.Common;
using _GAME.Scripts.Pools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Items
{
    public class Coin : MonoBehaviour, IPoolableItem<CoinType>
    {
        [Header("General")]
        [SerializeField] private CoinType _type;
        [SerializeField] private GameObject _model;
        [Header("Bounce Settings")]
        [SerializeField] private float _bounceForce = 5f;
        [SerializeField] private float _spreadRadius = 2f;
        [SerializeField] private float _gravity = 9.8f;
        [SerializeField] private float _bounceDamping = 0.6f;
        [SerializeField] private int _maxBounces = 3;
        
        [Header("Collision Settings")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private float _wallBounceMultiplier = 0.8f;
        [SerializeField] private LayerMask _obstacleLayer;

        [Header("Attraction Settings")]
        [SerializeField]
        private Vector3 _attractOffset = Vector3.up;
        [SerializeField] private float _attractSpeed = 10f;
        [SerializeField] private float _attractAcceleration = 2f;
        
        private Vector3 _velocity;
        private int _bounceCount = 0;
        private bool _isGrounded = false;
        private bool _isAttracting = false;
        private Transform _target;
        private float _currentAttractSpeed;
        private float _groundLevel = 0f;
        
        public event Action<Coin> OnCollected;
        
        public CoinType Type => _type;
        

        public void Initialize(Vector3 spawnPosition, Vector3 direction)
        {
            transform.position = spawnPosition;
            _groundLevel = .1f;
            
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0f,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
            
            Vector3 spreadDirection = (direction + randomOffset).normalized;

            _velocity = (spreadDirection * _spreadRadius + Vector3.up * _bounceForce) * Random.Range(.5f, 1.5f);
            _bounceCount = 0;
            _isGrounded = false;
            _isAttracting = false;

            Show();
        }
        
        public void Show()
        {
            _model.SetActive(true);
        }
        
        public void Hide()
        {
            _model.SetActive(false);
        }

        private void Update()
        {
            if (_isAttracting)
            {
                AttractToTarget();
            }
            else if (!_isGrounded)
            {
                SimulatePhysics();
            }
        }

        private void SimulatePhysics()
        {
            _velocity.y -= _gravity * Time.deltaTime;
            
            Vector3 movement = _velocity * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
            
            if (CheckObstacleCollision(movement, out Vector3 hitNormal, out float hitDistance))
            {
                _velocity = Vector3.Reflect(_velocity, hitNormal) * _wallBounceMultiplier * Random.Range(.8f, 1.2f);
                
                newPosition = transform.position + _velocity.normalized * hitDistance;
            }
            
            if (newPosition.y <= _groundLevel && _velocity.y < 0)
            {
                newPosition.y = _groundLevel;
                _bounceCount++;
                
                if (_bounceCount < _maxBounces)
                {
                    _velocity.y = Mathf.Abs(_velocity.y) * _bounceDamping;
                    _velocity.x *= _bounceDamping;
                    _velocity.z *= _bounceDamping;
                    
                    _velocity *= Random.Range(.8f, 1.2f);
                }
                else
                {
                    _velocity = Vector3.zero;
                    _isGrounded = true;
                }
            }
            
            transform.position = newPosition;
            
            transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.World);
        }

        private bool CheckObstacleCollision(Vector3 movement, out Vector3 hitNormal, out float hitDistance)
        {
            hitNormal = Vector3.up;
            hitDistance = 0f;
            
            float checkDistance = movement.magnitude + _collisionRadius;
            
            if (Physics.SphereCast(
                transform.position, 
                _collisionRadius, 
                movement.normalized, 
                out RaycastHit hit, 
                checkDistance,
                _obstacleLayer))
            {
                hitNormal = hit.normal;
                hitDistance = hit.distance - _collisionRadius;
                return true;
            }
            
            return false;
        }

        public void StartAttraction(Transform target)
        {
            _target = target;
            _isAttracting = true;
            _currentAttractSpeed = _attractSpeed * Random.Range(.8f, 1.2f);;
        }

        private void AttractToTarget()
        {
            if (_target == null) return;

            Vector3 direction = ((_target.position + _attractOffset) - transform.position);
            float distance = direction.magnitude;

            if (distance < 0.5f)
            {
                Collect();
                return;
            }
            
            _currentAttractSpeed += _attractAcceleration * Time.deltaTime;
            
            transform.position += direction.normalized * _currentAttractSpeed * Time.deltaTime;
            
            transform.Rotate(Vector3.up, 720f * Time.deltaTime, Space.World);
        }

        private void Collect()
        {
            OnCollected?.Invoke(this);
        }
    }
}