using System.Collections;
using UnityEngine;

namespace _GAME.Scripts.Audio
{
    public class ManagedAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Coroutine _fadeCoroutine;
        private bool _isPooled = false;

        public AudioSource AudioSource => _audioSource;
        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
        public bool IsPooled => _isPooled;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Setup(AudioClip clip, float volume, float pitch, bool loop = false, bool spatial = false)
        {
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
            _audioSource.loop = loop;
            
            if (spatial)
            {
                _audioSource.spatialBlend = 1f;
                _audioSource.rolloffMode = AudioRolloffMode.Linear;
                _audioSource.maxDistance = 20f;
            }
            else
            {
                _audioSource.spatialBlend = 0f;
            }
        }

        public void Play()
        {
            _audioSource.Play();
            _isPooled = false;
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        public void FadeTo(float targetVolume, float duration, System.Action onComplete = null)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeCoroutine(targetVolume, duration, onComplete));
        }

        private IEnumerator FadeCoroutine(float targetVolume, float duration, System.Action onComplete)
        {
            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            _audioSource.volume = targetVolume;
            onComplete?.Invoke();
            _fadeCoroutine = null;
        }

        public void ReturnToPool()
        {
            _isPooled = true;
            _audioSource.Stop();
            gameObject.SetActive(false);
        }
    }
}
