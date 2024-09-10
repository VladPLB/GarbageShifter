using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME
{
    public class AutoDestroyByTime: MonoBehaviour
    {
        [SerializeField] private float _delay = 1f;

        private async void Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delay));
            Destroy(gameObject);
        }

        public static void AddTo(GameObject go, float delay = 1f)
        {
            var trigger = go.AddComponent<AutoDestroyByTime>();
            trigger._delay = delay;
        }
    }
}