using System;
using System.Collections;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class DroneBomberFireConditionBehaviour: BaseEnemyFireConditionBehaviour
    {
        [SerializeField] private GameObject _warningItem;

        private EnemyController _controller;
        private bool _isReady = false;
        private bool _isFire = false;

        public override bool IsFire(EnemyController controller)
        {
            _controller = controller;
            if (!_isReady)
            {
                if (_controller.Mover.IsAttackedDistance)
                {
                    _isReady = true;
                    StartCoroutine(Process());
                }
            }

            return _isFire;
        }

        private void OnEnable()
        {
            _isFire = false;
            _isReady = false;
            _warningItem.SetActive(false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator Process()
        {
            _warningItem.SetActive(true);
            yield return new WaitForSeconds(2f);
            _controller.Mover.JumpToPlayer(()=>_isFire = true);
            
        }
    }
}