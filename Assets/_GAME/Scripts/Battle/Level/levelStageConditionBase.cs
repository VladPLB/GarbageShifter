using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public abstract class levelStageConditionBase : MonoBehaviour
    {
        protected LevelStage _stage;
        protected bool _isSkipStage = false;
        protected bool _isInit = false;
        
        public abstract bool IsNext { get; }

        public virtual void Setup(LevelStage stage)
        {
            _stage = stage;
            _isInit = true;
        }

        protected virtual void Update()
        {
            if(!_isInit)
                return;
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