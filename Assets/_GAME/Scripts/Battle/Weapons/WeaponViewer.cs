using Unity.Mathematics;
using UnityEngine;

namespace _GAME.Scripts.Battle.Weapons
{
    public class WeaponViewer: MonoBehaviour
    {
        [SerializeField] private Vector3 _readyPosition;
        [SerializeField] private Vector3 _readyRotation;
        
        [SerializeField] private Vector3 _freePosition;
        [SerializeField] private Vector3 _freeRotation;

        private Transform _readyParent = null;
        private Transform _freeParent = null;
        
        public void SetupOverrideParents(Transform readyStateParent, Transform freeStateParent)
        {
            _readyParent = readyStateParent;
            _freeParent = freeStateParent;
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

        public void BattleStop()
        {
            if(_freeParent)
            {
                transform.parent = _freeParent;
                transform.localPosition = _freePosition;
                transform.localRotation = Quaternion.Euler(_freeRotation);
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