using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class AllyTargetFinderRandom: BaseAllyTargetFinder
    {
        [SerializeField] private Transform _aimPoint;
        private AllyController _controller = null;
        private UnitsController _unitsController;
        private EnemyController _target;
        private bool _isFire = false;
        
        public override void Init(AllyController controller)
        {
            _controller = controller;
            var levelController = Core.Get<LevelController>();
            _unitsController = levelController.UnitsController;
        }

        private void Update()
        {
            if (!IsActive())
            {
                _target = null;
                return;
            }
            
            if (_target != null)
            {
                if (_target.Health.Current <= 0)
                {
                    _target = null;
                }
            }
            else
            {
                _isFire = false;
                var enemies = _unitsController.ActiveUnits;
                _target = enemies.GetRandomItem();
            }
        }

        private void LateUpdate()
        {
            if (!IsActive())
                return;
            var aimPosition = _target != null ?  _target.TargetPoint : _controller.transform.position + _controller.Mover.Forward + Vector3.up;
            _aimPoint.position = aimPosition;

        }

        private bool IsActive() => _controller != null && _controller.Mover.IsActive && _controller.Mover.IsStopped;

        public override bool IsFire()
        {
            if (!IsActive() || _target == null)
            {
                _isFire = false;
                return false;
            }
            
            if (!_isFire)
            {
                _controller.Data.WeaponData.ResetFireTime(Time.time);
            }

            _isFire = true;
            return true;
        }

        public override Transform GetTarget()
        {
            return _target != null? _target.transform: null;
        }
    }
}