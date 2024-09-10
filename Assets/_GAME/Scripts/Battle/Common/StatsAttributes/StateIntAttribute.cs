using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    [Serializable]
    public class StateIntAttribute
    {
        public int Current;
        public int Max;

        public event Action<int> OnChangeValue;

        public float Value => Max>0?(float)Current / Max : 0;

        public void Set(int max)
        {
            Max = max;
            Reset();
        }

        public void Reset()
        {
            Current = Max;
        }

        public void Add(int value)
        {
            value = Mathf.Abs(value);
            Change(value);
        }

        public void Remove(int value)
        {
            value = Mathf.Abs(value) * -1;
            Change(value);
        }
        
        private void Change(int value)
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