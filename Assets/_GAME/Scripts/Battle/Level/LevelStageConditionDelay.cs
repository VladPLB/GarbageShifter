using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStageConditionDelay : levelStageConditionBase
    {
        [SerializeField] protected float _delay;

        protected float _delayTime = 0f;
        public override bool IsNext => _isSkipStage || _delayTime<=0;

        public override void Setup(LevelStage stage)
        {
            _delayTime = _delay;
        }

        protected override void Update()
        {
            base.Update();
            if (!IsNext)
            {
                _delayTime -= Time.deltaTime;
            }
        }
    }
}