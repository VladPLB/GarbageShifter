using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Transform _lookPoint;
    [SerializeField] private Transform _precilePoint;
    [SerializeField] private LayerMask _precileMask;
    [SerializeField] private Vector3 _defaultDirection = new Vector3(0,0,1);
    [SerializeField] private float _sencetivity = 1f;
    [SerializeField] private Vector2 _rotateDeadZoneX = new Vector2(-75f,75f);
    [SerializeField] private Vector2 _rotateDeadZoneY = new Vector2(-160, 130);

    [SerializeField] private TwoBoneIKConstraint _armRigL;
    [SerializeField] private Transform _armIkL;
    [SerializeField] private TwoBoneIKConstraint _armRigR;
    [SerializeField] private Transform _armIkR;
    [SerializeField] private MultiAimConstraint _bodyRig;
    [SerializeField] private AimConstraint _headRig;
    [SerializeField] private Transform _targetIk;
    [SerializeField] private Rig _rig;

    private bool _isActive = false;
    private GameInput _gameInput;
    private Vector3 rotate;
    private Vector3 targetEulerAngles;
    private bool _isAimLock = false;

    public Vector3 AimPoint => _precilePoint.position;

    public void Setup(GameInput input)
    {
        _gameInput = input;
        EventBus.Subscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
    }

    private void OnAimLock(AimLockEvent lockEvent)
    {
        _isAimLock = lockEvent.IsLock;
    }

    public void SetActive(bool isActive)
    {
        SetAim();
        _isActive = isActive;
        _rig.weight = isActive ? 1f : 0f;

        ResetCameraDirection();
    }
    
    private void SetAim()
    {
        SetAim(_armIkL, _armIkR, _targetIk);
    }
    
    public void SetAim(Transform armL, Transform armR, Transform target)
    {
        _armRigL.data.target = armL;
        _armRigR.data.target = armR;
        _bodyRig.data.sourceObjects = new WeightedTransformArray() { new WeightedTransform(target, .8f) };
        var cSource = new ConstraintSource() { sourceTransform = target, weight = 1f };
        _headRig.SetSource(0, cSource);
    }

    private void ResetCameraDirection()
    {
        _lookPoint.transform.localRotation = Quaternion.Euler(_defaultDirection);
        targetEulerAngles = _lookPoint.localEulerAngles;
    }

    void Update()
    {
        if(!_isActive || _isAimLock)
            return;
        
        var delta = _gameInput.Game.PointerDelta.ReadValue<Vector2>();
        rotate = new Vector3(delta.y, delta.x * -1f, 0) * _sencetivity;
        targetEulerAngles -= rotate;
        targetEulerAngles.x = Mathf.Clamp(targetEulerAngles.x, _rotateDeadZoneX.x, _rotateDeadZoneX.y);
        targetEulerAngles.y = Mathf.Clamp(targetEulerAngles.y, _rotateDeadZoneY.x, _rotateDeadZoneY.y);
       targetEulerAngles.z = 0;
       _lookPoint.localEulerAngles = targetEulerAngles;
    }

    private void LateUpdate()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        if (Physics.Raycast(ray, out var raycastHit, float.PositiveInfinity, _precileMask))
        {
            _precilePoint.transform.position = raycastHit.point;
        }
        else
        {
            _precilePoint.transform.position = ray.origin + ray.direction * 10f;
        }
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
    }
}
