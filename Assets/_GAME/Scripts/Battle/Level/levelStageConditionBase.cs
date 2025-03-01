using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public abstract class levelStageConditionBase : MonoBehaviour
    {
        protected LevelStage _stage;
        protected bool _isSkipStage = false;
        
        public abstract bool IsNext { get; }

        public virtual void Setup(LevelStage stage)
        {
            _stage = stage;
        }

        protected virtual void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    _isSkipStage = true;
                }
            }
        }
    }
}