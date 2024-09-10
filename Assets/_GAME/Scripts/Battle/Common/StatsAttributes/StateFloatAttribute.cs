using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    [Serializable]
    public class StateFloatAttribute
    {
        public float Current;
        public float Max;

        public event Action<float> OnChangeValue;

        public float Value => Max>0?Current / Max : 0;

        public void Set(float max)
        {
            Max = max;
            Reset();
        }

        public void Reset()
        {
            Current = Max;
        }

        public void Add(float value)
        {
            value = Mathf.Abs(value);
            Change(value);
        }

        public void Remove(float value)
        {
            value = Mathf.Abs(value) * -1;
            Change(value);
        }
        
        private void Change(float value)
        {
            var previous = Current;
            Current += value;
            Current = Mathf.Clamp(Current, 0, Max);
            var delta = Current - previous;
            if (delta != 0)
            {
                OnChangeValue?.Invoke(delta);
            }
        }
    }
}