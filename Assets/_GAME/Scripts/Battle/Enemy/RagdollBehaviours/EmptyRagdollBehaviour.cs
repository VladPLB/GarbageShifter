using Cysharp.Threading.Tasks;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class EmptyRagdollBehaviour: BaseRagdollBehaviour
    {
        [SerializeField]
        private GameObject _model;

        public override void Setup()
        {
            _model.SetActive(true);
        }
        
        public override async UniTask Show()
        {
            _model.SetActive(false);
        }
        
        public override async UniTask ShowWithHit(Vector3 hitPoint, float force)
        {
            await Show();
        }
        
        public override async UniTask ShowWithExplosion(Vector3 direction, float force)
        {
            await Show();
        }
    }
}