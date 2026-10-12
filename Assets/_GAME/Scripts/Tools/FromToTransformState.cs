using System;
using UnityEngine;

namespace _GAME.Scripts.Tools
{
    [Serializable]
    public class FromToTransformState
    {
        [SerializeField]  private Transform _transform;
        [SerializeField] private Vector3 _fromPos;
        [SerializeField] private Quaternion _fromRot;
        [SerializeField] private Vector3 _toPos;
        [SerializeField] private Quaternion _toRot;

        public void Apply(float val)
        {
            _transform.localPosition = Vector3.Lerp(_fromPos, _toPos, val);
            _transform.localRotation = Quaternion.Lerp(_fromRot, _toRot, val);
        }

        public void SetFrom()
        {
            _fromPos = _transform.localPosition;
            _fromRot = _transform.localRotation;
        }

        public void SetTo()
        {
            _toPos = _transform.localPosition;
            _toRot = _transform.localRotation;
        }
    }
}