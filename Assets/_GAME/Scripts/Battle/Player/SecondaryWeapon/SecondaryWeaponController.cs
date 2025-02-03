using System;
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
        private bool _isFire = false;
        private Vector3 _previousWeaponHolderOffset;

        public void Setup(Player player,PlayerAim aim)
        {
            _aim = aim;
            _fill.Progress = 0f;
            _isReady = false;
            _weaponHolder.transform.position = _inactiveWeaponOffset;
            _weapon.Setup(player);
            SetWeaponTransform();
            _fillAnimator.Rebind();
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
                _fill.Progress += .1f;
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
                position =  Vector3.Lerp(position,_previousWeaponHolderOffset, .03f);
                weaponHolderTransform.position = position;
                weaponHolderTransform.rotation= Quaternion.Lerp(weaponHolderTransform.rotation,
                    Quaternion.LookRotation((_aim.AimPoint - position).normalized), .1f);
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
    }
}