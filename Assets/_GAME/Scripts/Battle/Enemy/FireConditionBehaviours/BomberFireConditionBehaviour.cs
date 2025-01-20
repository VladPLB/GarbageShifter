using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class BomberFireConditionBehaviour: BaseEnemyFireConditionBehaviour
    {
        public override bool IsFire(EnemyController controller)
        {
            return controller.Mover.IsStoppingDistance;
        }
    }
}