using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class EnemyViewer: MonoBehaviour
    {
        private static readonly int StopKey = Animator.StringToHash("Stop");
        private static readonly int RunKey = Animator.StringToHash("Run");
        private static readonly int FireKey = Animator.StringToHash("Fire");
        private static readonly int DeadKey = Animator.StringToHash("Dead");
        private static readonly int DeadTypeKey = Animator.StringToHash("DeadType");

        [SerializeField] private Animator _animator;

        public void Setup()
        {
            _animator.Rebind();
        }

        public void Stop()
        {
            _animator.SetTrigger(StopKey);
        }

        public void Run()
        {
            _animator.SetTrigger(RunKey);
        }

        public void Fire()
        {
            _animator.SetTrigger(FireKey);
        }
        
        public void Dead()
        {
            _animator.SetFloat(DeadTypeKey, Mathf.RoundToInt(Random.Range(0f,1f)));
            _animator.SetTrigger(DeadKey);
        }
    }
}