using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Audio
{
    public class AudioSourcePool
    {
        private readonly Transform _container;
        private readonly Queue<ManagedAudioSource> _pool = new Queue<ManagedAudioSource>();
        private readonly List<ManagedAudioSource> _active = new List<ManagedAudioSource>();
        private readonly int _initialSize;

        public AudioSourcePool(Transform container, int initialSize = 10)
        {
            _container = container;
            _initialSize = initialSize;
            
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewSource();
            }
        }

        private ManagedAudioSource CreateNewSource()
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.SetParent(_container);
            go.SetActive(false);
            
            var source = go.AddComponent<ManagedAudioSource>();
            _pool.Enqueue(source);
            
            return source;
        }

        public ManagedAudioSource Get()
        {
            if (_pool.Count == 0)
            {
                CreateNewSource();
            }

            var source = _pool.Dequeue();
            source.gameObject.SetActive(true);
            _active.Add(source);
            
            return source;
        }

        public void Return(ManagedAudioSource source)
        {
            if (_active.Contains(source))
            {
                _active.Remove(source);
                source.ReturnToPool();
                _pool.Enqueue(source);
            }
        }

        public void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var source = _active[i];
                if (!source.IsPlaying && !source.AudioSource.loop)
                {
                    Return(source);
                }
            }
        }

        public void StopAll()
        {
            foreach (var source in _active)
            {
                source.Stop();
            }
        }
    }
}
