using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Weapons
{
    public class WeaponViewer: MonoBehaviour
    {
        [SerializeField] private bool _usePhysicsDrop = false;
        [SerializeField] private Collider _collider;
        [SerializeField] private Vector3 _readyPosition;
        [SerializeField] private Vector3 _readyRotation;
        
        [SerializeField] private Vector3 _freePosition;
        [SerializeField] private Vector3 _freeRotation;
        
        private Transform _defaultParent;
        private Vector3 _defaultPosition;
        private Quaternion _defaultRotation;

        private Transform _readyParent = null;
        private Transform _freeParent = null;
        
        public void Setup(Transform readyStateParent = null, Transform freeStateParent = null)
        {
            _defaultParent = transform.parent;
            _defaultPosition = transform.localPosition;
            _defaultRotation = transform.localRotation;
            
            _readyParent = readyStateParent;
            _freeParent = freeStateParent;

            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        public async void DoPhysicsDrop(float duration)
        {
            var rigidbody = gameObject.AddComponent<Rigidbody>();
            _collider ??= gameObject.AddComponent<BoxCollider>();
            rigidbody.AddForce(Vector3.up + Random.insideUnitSphere * 10f, ForceMode.Impulse);
            transform.parent = null;
            _collider.enabled = true;
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            Destroy(rigidbody);
            _collider.enabled = false;
            transform.parent = _defaultParent;
            transform.localPosition = _defaultPosition;
            transform.localRotation = _defaultRotation;
        }

        public void BattleReady()
        {
            if(_readyParent)
            {
                transform.parent = _readyParent;
                transform.localPosition = _readyPosition;
                transform.localRotation = Quaternion.Euler(_readyRotation);
            }
        }

        public void BattleStop(bool isForce)
        {
            if(_freeParent)
            {
                transform.parent = _freeParent;
                transform.localPosition = _freePosition;
                transform.localRotation = Quaternion.Euler(_freeRotation);
            }
            else 
            {
                if(isForce)
                {
                    transform.parent = _defaultParent;
                    transform.localPosition = _defaultPosition;
                    transform.localRotation = _defaultRotation;
                }
                else if (_usePhysicsDrop)
                {
                    DoPhysicsDrop(5f);
                }
            }
            
        }

        #if UNITY_EDITOR
        [ContextMenu("SaveReadyState")]
        private void SaveReadyState()
        {
            _readyPosition = transform.localPosition;
            _readyRotation = transform.localRotation.eulerAngles;
        }
        
        [ContextMenu("SaveFreeState")]
        private void SaveFreeState()
        {
            _freePosition = transform.localPosition;
            _freeRotation = transform.localRotation.eulerAngles;
        }
        #endif
    }
}