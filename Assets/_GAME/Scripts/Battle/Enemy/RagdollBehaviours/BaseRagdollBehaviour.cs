using Cysharp.Threading.Tasks;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public abstract class BaseRagdollBehaviour: MonoBehaviour
    {
        public abstract void Setup();

        public abstract UniTask Show();

        public abstract UniTask ShowWithHit(Vector3 hitPoint, float force);

        public abstract UniTask ShowWithExplosion(Vector3 direction, float force);
    }
}