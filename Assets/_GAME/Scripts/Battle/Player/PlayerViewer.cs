using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class PlayerViewer: MonoBehaviour
    {
        private static readonly int StopKey = Animator.StringToHash("Stop");
        private static readonly int RunKey = Animator.StringToHash("Run");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int FlyKey = Animator.StringToHash("Fly");
        private static readonly int FireKey = Animator.StringToHash("Fire");
        private static readonly int SecondaryFireKey = Animator.StringToHash("SecondaryFire");
        private static readonly int DeadKey = Animator.StringToHash("Dead");
        private static readonly int ReviveKey = Animator.StringToHash("Revive");
        private static readonly int VictoryKey = Animator.StringToHash("Victory");
        
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _flyTrustEffect;

        public void Stop()
        {
            _animator.SetTrigger(StopKey);
            _flyTrustEffect.gameObject.SetActive(false);
        }
        
        public void Run()
        {
            _animator.SetTrigger(RunKey);
        }

        public void Jump()
        {
            _animator.SetTrigger(JumpKey);
        }
        
        public void Fly()
        {
            _animator.SetTrigger(FlyKey);
            _flyTrustEffect.gameObject.SetActive(true);
        }

        public void Fire(bool isDefault = true)
        {
            _animator.SetTrigger(  isDefault?FireKey:SecondaryFireKey);
        }
        
        public void Dead()
        {
            _animator.SetTrigger(DeadKey);
        }
        
        public void Revive()
        {
            _animator.SetTrigger(ReviveKey);
        }
        
        public void Victory()
        {
            _animator.SetTrigger(VictoryKey);
        }
    }
}