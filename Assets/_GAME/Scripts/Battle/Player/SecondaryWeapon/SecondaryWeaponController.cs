using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    public class SecondaryWeaponController : MonoBehaviour
    {
        [SerializeField] private FillPlane _fill;
        [SerializeField] private Animator _fillAnimator;
        [SerializeField] private Transform _weaponBase;
        [SerializeField] private Transform _weaponHolder;
        [SerializeField] private Transform _weaponPivot;
        [SerializeField] private Vector3 _activeWeaponOffset;
        [SerializeField] private Vector3 _inactiveWeaponOffset;
        [SerializeField] private Vector3 _inactiveWeaponRotate;
        
        [SerializeField]
        private SecondaryWeaponBase _weapon;

        private PlayerAim _aim;
        private bool _isReady = false;
        private Vector3 _previousWeaponHolderOffset;
        private float _maxValue = 1f;

        public bool IsReady => _weapon.IsReady && !_weapon.IsFire;

        public void Setup(Player player,PlayerAim aim)
        {
            _aim = aim;
            _fill.Progress = 0f;
            _maxValue = 1f;
            _isReady = false;
            _weaponHolder.transform.position = _inactiveWeaponOffset;
            _weapon.Setup(player);
            SetWeaponTransform();
            _fillAnimator.Rebind();
            EventBus.Subscribe<SeccondaryMaxValueEvent>(OnMaxValueChange, EventBus.EventRegion.GAMEPLAY);
        }

        private void OnMaxValueChange(SeccondaryMaxValueEvent maxValueEvent)
        {
            _maxValue = Mathf.Clamp01(maxValueEvent.MaxValue);
        }

        private void SetWeaponTransform()
        {
            var weaponTransform= _weapon.transform;
            weaponTransform.parent = _weaponPivot;
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localScale = Vector3.one;
            weaponTransform.localRotation = Quaternion.identity;
        }

        public void OnHit()
        {
            if(_fill.Progress<1f)
            {
                _fill.Progress = Mathf.Clamp(_fill.Progress + .03f, 0, _maxValue);
                if (_fill.Progress < 1f)
                {
                    _fillAnimator.SetTrigger("Added");
                }
                else
                {
                    Show();
                }
            }
        }

        public async void OnFire()
        {
            if(!_weapon.IsReady || _weapon.IsFire)
                return;
            
            
            _weapon.OnFire();
            _fill.Progress = 0f;
            await UniTask.WaitWhile(() => _weapon.IsFire);
            Hide();
        }

        private void LateUpdate()
        {
            var weaponHolderTransform = _weaponHolder.transform;

            if (_isReady)
            {
                _previousWeaponHolderOffset = Vector3.Lerp(_previousWeaponHolderOffset, _weaponBase.TransformPoint(_activeWeaponOffset), .05f);
                var position = weaponHolderTransform.position;
                position =  Vector3.Lerp(_previousWeaponHolderOffset, position, .1f);
                weaponHolderTransform.position = position;
                weaponHolderTransform.rotation = Quaternion.LookRotation((_aim.AimPoint - position).normalized);
            }
            else
            {
                weaponHolderTransform.localPosition =  Vector3.Lerp(weaponHolderTransform.localPosition,_inactiveWeaponOffset, .03f);
                weaponHolderTransform.localRotation= Quaternion.Lerp(weaponHolderTransform.localRotation,
                    Quaternion.Euler(_inactiveWeaponRotate), .1f);
            }

            
        }

        private async void Show()
        {
            _fillAnimator.SetBool("IsFull", true);
            await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            _isReady = true;
            _previousWeaponHolderOffset = _weaponBase.TransformPoint(_inactiveWeaponOffset);
            await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            _weapon.Show();
        }
        private async void Hide()
        {
            _fillAnimator.SetBool("IsFull", false);
            await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            _isReady = false;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SeccondaryMaxValueEvent>(OnMaxValueChange, EventBus.EventRegion.GAMEPLAY);
        }
    }
}