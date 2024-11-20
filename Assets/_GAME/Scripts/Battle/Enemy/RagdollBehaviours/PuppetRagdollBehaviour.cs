using Cysharp.Threading.Tasks;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class PuppetRagdollBehaviour: BaseRagdollBehaviour
    {
        [SerializeField]
        private RagDollController _ragdoll;

        public override void Setup()
        {
            _ragdoll.Setup();
        }
        
        public override async UniTask Show()
        {
            await _ragdoll.Show();
        }
        
        public override async UniTask ShowWithHit(Vector3 hitPoint, float force)
        {
            await _ragdoll.ShowWithHit(hitPoint, force);
        }
        
        public override async UniTask ShowWithExplosion(Vector3 direction, float force)
        {
            await _ragdoll.ShowWithExplosion(direction, force);
        }
    }
}