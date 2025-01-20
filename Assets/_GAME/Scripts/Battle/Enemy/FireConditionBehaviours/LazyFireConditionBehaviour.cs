using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class LazyFireConditionBehaviour: BaseEnemyFireConditionBehaviour
    {
        private bool _isFire = false;
        
        public override bool IsFire(EnemyController controller)
        {
            if (!controller.Mover.IsAttackDistance)
            {
                _isFire = false;
                return false;
            }
            
            if (!_isFire)
            {
                controller.Data.WeaponData.ResetFireTime(Time.time);
            }

            _isFire = true;
            return true;
        }
    }
}