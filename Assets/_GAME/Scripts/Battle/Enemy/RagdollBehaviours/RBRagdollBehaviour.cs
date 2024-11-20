using Cysharp.Threading.Tasks;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class RBRagdollBehaviour: BaseRagdollBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        public override void Setup()
        {
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        public override async UniTask Show()
        {
            _rigidbody.useGravity = true;
            _rigidbody.constraints = RigidbodyConstraints.None;
        }
        
        public override async UniTask ShowWithHit(Vector3 hitPoint, float force)
        {
            await Show();
            Vector3 direction = ( _rigidbody.position - hitPoint ).normalized;
            _rigidbody.AddForceAtPosition(direction * force, hitPoint, ForceMode.Impulse);
        }
        
        public override async UniTask ShowWithExplosion(Vector3 direction, float force)
        {
            await Show();
            _rigidbody.AddForce(direction * force * 5f, ForceMode.Impulse);
        }
    }
}