using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public abstract class BaseEnemyFireConditionBehaviour: MonoBehaviour
    {
        public abstract bool IsFire(EnemyController controller);
    }
}