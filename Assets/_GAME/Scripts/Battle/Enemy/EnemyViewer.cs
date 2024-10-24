using UnityEngine;

namespace _GAME.Scripts.Battle.Player
{
    public class EnemyViewer: MonoBehaviour
    {
        private static readonly int RunKey = Animator.StringToHash("Run");
        private static readonly int FireKey = Animator.StringToHash("Fire");
        private static readonly int HitKey = Animator.StringToHash("Hit");
        private static readonly int DeadKey = Animator.StringToHash("Dead");
        private static readonly int DeadTypeKey = Animator.StringToHash("DeadType");

        [SerializeField] private Animator _animator;

        public void Setup()
        {
            _animator.Rebind();
        }

        public void Run(float speed)
        {
            _animator.SetFloat(RunKey, speed);
        }
        
        public void Hit()
        {
            _animator.SetTrigger(HitKey);
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