using System;
using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Audio
{
    [Serializable]
    public class AmbientSound
    {
        public AmbientType Type;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.5f;
        public bool Is3D = false;
        public Vector3 Position;
    }

    public class AmbientManager
    {
        private readonly Dictionary<AmbientType, ManagedAudioSource> _activeAmbients = new Dictionary<AmbientType, ManagedAudioSource>();
        private readonly Dictionary<AmbientType, Transform> _movedAmbients = new Dictionary<AmbientType, Transform>();
        private readonly Transform _container;
        private readonly AudioSourcePool _pool;

        public AmbientManager(Transform container, AudioSourcePool pool)
        {
            _container = container;
            _pool = pool;
        }

        public void PlayAmbient(AmbientSound ambient, Transform anchor = null)
        {
            if (_activeAmbients.ContainsKey(ambient.Type))
            {
                StopAmbient(ambient.Type);
            }

            var source = _pool.Get();
            if (anchor != null)
            {
                _movedAmbients.Add(ambient.Type, anchor);
                source.transform.position = anchor.position;
            }
            else
            {
                source.transform.position = ambient.Position;
            }
            
            source.Setup(ambient.Clip, ambient.Volume, 1f, loop: true, spatial: ambient.Is3D);
            source.Play();

            _activeAmbients[ambient.Type] = source;
        }

        public void StopAmbient(AmbientType type, float fadeTime = 0f)
        {
            if (_activeAmbients.TryGetValue(type, out var source))
            {
                if (fadeTime > 0)
                {
                    source.FadeTo(0f, fadeTime, () => 
                    {
                        _pool.Return(source);
                    });
                }
                else
                {
                    _pool.Return(source);
                }
                
                _activeAmbients.Remove(type);
                _movedAmbients.Remove(type);
            }
        }

        public void StopAll(float fadeTime = 0f)
        {
            var types = new List<AmbientType>(_activeAmbients.Keys);
            foreach (var type in types)
            {
                StopAmbient(type, fadeTime);
            }
        }

        public void SetAmbientVolume(AmbientType type, float volume)
        {
            if (_activeAmbients.TryGetValue(type, out var source))
            {
                source.AudioSource.volume = volume;
            }
        }

        public void UpdatePosition(AmbientType type, Vector3 position)
        {
            if (_activeAmbients.TryGetValue(type, out var source))
            {
                source.transform.position = position;
            }
        }

        public void Update()
        {
            foreach (var source in _movedAmbients)
            {
                UpdatePosition(source.Key, source.Value.position);
            }
        }
    }
}
