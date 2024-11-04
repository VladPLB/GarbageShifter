using Cysharp.Threading.Tasks;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class RagDollController: FimpossibleComponent
    {
        [SerializeField]
        private RagdollAnimator2 _ragdoll;

        public void Setup()
        {
            _ragdoll.Mecanim.enabled = true;
            _ragdoll.enabled = false;
        }
        
        public async UniTask Show()
        {
            _ragdoll.enabled = true;
            await UniTask.DelayFrame(2);
            _ragdoll.User_UpdateAllBonesParametersAfterManualChanges();
            _ragdoll.User_SwitchFallState( RagdollHandler.EAnimatingMode.Falling );
            _ragdoll.User_DisableMecanimAfter(.5f);
        }
        
        public async UniTask Show(Vector3 hitPoint, float force)
        {
            await Show();
            Hit(hitPoint, force);
        }

        private void Hit(Vector3 hitPoint, float force)
        {
            Rigidbody nearest = _ragdoll.User_GetNearestRagdollRigidbodyToPosition( hitPoint, true, ERagdollChainType.Core );

            if( nearest == null)
                return;

            Vector3 dir = ( nearest.position - hitPoint ).normalized;
            _ragdoll.User_AddRigidbodyImpact( nearest, ( dir + new Vector3( 0f, .4f, 0f ) ) * ( force ), 0.14f, ForceMode.Impulse, 0.06f );

        }
    }
}