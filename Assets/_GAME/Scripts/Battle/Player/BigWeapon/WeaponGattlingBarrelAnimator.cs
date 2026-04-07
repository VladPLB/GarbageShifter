using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponGattlingBarrelAnimator : MonoBehaviour
{
    [SerializeField] private Transform _barrel;
    [SerializeField] private Vector3 _rotateDirection;
    [SerializeField] private float _rotateSpeed = 1f;
    [SerializeField] private float _rotateSpeedUpDamping = 1f;
    [SerializeField] private float _rotateSpeedDownDamping = 1f;
    [SerializeField] private List<GameObject> _activeItems = new();
    [SerializeField] private Transform _temperatureInactive;
    [SerializeField] private DecalProjector _temperatureDecal;
    [SerializeField] private float _temperatureDamping = 1f;
    [SerializeField] private bool _IsFire = true;
    
    protected float _currentSpeed = 0;
    protected float _currentTemperature = 0;
    protected bool _isActive = false;
    protected bool _previousState = false;
    
    public bool IsFireReady => _currentSpeed >= _rotateSpeed;

    public bool IsFire { get; set; }

    private void Start()
    {
        _currentSpeed = 0;
        _currentTemperature = 0;
        _isActive = false;
        _previousState = !_isActive;
        
        UpdateActiveItemsAndTemperatureState();
    }

    private void Update()
    {
        _isActive = IsFireReady;
        
        if(IsFire)
        {
            _currentSpeed = Mathf.Min( Mathf.Lerp(_currentSpeed , _rotateSpeed* 1.1f,
                Time.deltaTime * _rotateSpeedUpDamping), _rotateSpeed);
        }
        else
        {
            _currentSpeed =  Mathf.Max(  Mathf.Lerp(_currentSpeed, -0.1f,
                Time.deltaTime * _rotateSpeedDownDamping), 0f);
        }
        if(IsFireReady && IsFire)
        {
            _currentTemperature = Mathf.Lerp(_currentTemperature, 1,
                Time.deltaTime * _temperatureDamping);
        }
        else
        {
            _currentTemperature = Mathf.Lerp(_currentTemperature, 0,
                Time.deltaTime * _temperatureDamping);
        }
        _barrel.Rotate(_rotateDirection * _currentSpeed * Time.deltaTime);
        
        UpdateActiveItemsAndTemperatureState();
    }

    private void UpdateActiveItemsAndTemperatureState()
    {
        if(_previousState!=_isActive)
        {
            _previousState = _isActive;
            _activeItems.ForEach(item => item.SetActive(_isActive));
        }
        
        _temperatureDecal.fadeFactor = _currentTemperature;
        _temperatureInactive.localScale = IsFireReady?Vector3.zero : Vector3.one * _currentTemperature;
    }
}
