using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public abstract class BaseAllyTargetFinder: MonoBehaviour
    {
        public abstract void Init(AllyController controller);
        public abstract bool IsFire();
        public abstract Transform GetTarget();
    }
}