using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Audio
{
    [Serializable]
    public class SoundClip
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 3f)] public float Pitch = 1f;
        [Range(0f, 1f)] public float RandomVolume = 0f;
        [Range(0f, 0.5f)] public float RandomPitch = 0f;

        public float GetRandomVolume() => Volume + UnityEngine.Random.Range(-RandomVolume, RandomVolume);
        public float GetRandomPitch() => Pitch + UnityEngine.Random.Range(-RandomPitch, RandomPitch);
    }

    [Serializable]
    public class SoundGroup
    {
        public string Name;
        public SoundClip[] Clips;
        [Range(0f, 1f)] public float GroupVolume = 1f;

        public SoundClip GetRandomClip()
        {
            if (Clips == null || Clips.Length == 0) return null;
            return Clips[UnityEngine.Random.Range(0, Clips.Length)];
        }
    }

    public enum MusicTrack
    {
        None,
        Menu,
        Battle,
        Boss,
        Victory,
        Defeat,
        Tutorial
    }

    public enum SoundType
    {
        Shot = 0,
        Explosion = 1,
        Hit =2,
        LargeHit =3,
        LargeExplosion =4,
        Coin = 5,
        EnemyDeath_human = 6,
        EnemyDeath_droid = 7,
        EnemyDeath_alien = 8,
        PlayerDamage = 9,
        DialogTap = 10,
        ShowCommunicator =11,
        HideCommunicator=12,
        FootStep = 13,
        Notification =14,
        DoorOpen = 15,
        DoorBigOpen = 16,
        DoorSmallOpen = 17,
    }

    public enum AmbientType
    {
        All,
        Hower,
        Alarm,
        Booble,
        Battle,
        Nature,
        Trust,
    }
    public class AudioManager : MonoBehaviour, IRuntimeSetup
    {
        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _ambientVolume = 0.5f;
        [SerializeField] private float _musicCrossfadeTime = 1f;
        
        [Header("Music Tracks")]
        [SerializeField] private MusicTrackData[] _musicTracks;
        
        [Header("Sound Effects")]
        [SerializeField] private SoundTypeData[] _soundEffects;
        
        [Header("Ambient Sounds")]
        [SerializeField] private AmbientSound[] _ambientSounds;
        
        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;

        private ManagedAudioSource _currentMusic;
        private ManagedAudioSource _crossfadeMusic;
        private MusicTrack _currentMusicTrack = MusicTrack.None;
        
        private AudioSourcePool _sfxPool;
        private AmbientManager _ambientManager;
        
        private Transform _musicContainer;
        private Transform _sfxContainer;
        private Transform _ambientContainer;

        private Dictionary<MusicTrack, AudioClip> _musicDict = new Dictionary<MusicTrack, AudioClip>();
        private Dictionary<SoundType, SoundGroup> _sfxDict = new Dictionary<SoundType, SoundGroup>();
        private Dictionary<AmbientType, AmbientSound> _ambientDict = new Dictionary<AmbientType, AmbientSound>();

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public float AmbientVolume
        {
            get => _ambientVolume;
            set => _ambientVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            Core.Registry(this);
        }
        
        public void RuntimeSetup()
        {
            CreateContainers();
            InitializePools();
            InitializeDictionaries();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            EventBus.Subscribe<MusicPlayEvent>(PlayMusic, EventBus.EventRegion.GLOBAL);
            EventBus.Subscribe<AmbientPlayEvent>(PlayAmbient, EventBus.EventRegion.GLOBAL);
            EventBus.Subscribe<AmbientStopEvent>(StopAmbient, EventBus.EventRegion.GLOBAL);
            EventBus.Subscribe<SoundPlayEvent>(PlaySound, EventBus.EventRegion.GLOBAL);
        }

        private void CreateContainers()
        {
            _musicContainer = new GameObject("Music").transform;
            _musicContainer.SetParent(transform);
            
            _sfxContainer = new GameObject("SFX").transform;
            _sfxContainer.SetParent(transform);
            
            _ambientContainer = new GameObject("Ambient").transform;
            _ambientContainer.SetParent(transform);
        }

        private void InitializePools()
        {
            _sfxPool = new AudioSourcePool(_sfxContainer, _initialPoolSize);
            _ambientManager = new AmbientManager(_ambientContainer, new AudioSourcePool(_ambientContainer, 5));
        }

        private void InitializeDictionaries()
        {
            foreach (var track in _musicTracks)
            {
                _musicDict[track.Track] = track.Clip;
            }

            foreach (var sound in _soundEffects)
            {
                _sfxDict[sound.Type] = sound.Group;
            }

            foreach (var ambient in _ambientSounds)
            {
                _ambientDict[ambient.Type] = ambient;
            }
        }

        private void Update()
        {
            _sfxPool.Update();
            _ambientManager.Update();
        }

        #region Music
        
        public static void Play(MusicTrack track)
        {
            EventBus.Push(new MusicPlayEvent(track), EventBus.EventRegion.GLOBAL);
        }
        
        public void PlayMusic(MusicPlayEvent e) => PlayMusic(e.MusicTrack, true);

        public void PlayMusic(MusicTrack track, bool crossfade = true)
        {
            if (track == _currentMusicTrack && _currentMusic != null && _currentMusic.IsPlaying)
                return;

            if (!_musicDict.TryGetValue(track, out AudioClip clip))
            {
                Debug.LogWarning($"Music track {track} not found!");
                return;
            }

            if (crossfade && _currentMusic != null)
            {
                StartCoroutine(CrossfadeMusic(clip, track));
            }
            else
            {
                PlayMusicImmediate(clip, track);
            }
        }

        private void PlayMusicImmediate(AudioClip clip, MusicTrack track)
        {
            if (_currentMusic != null)
            {
                Destroy(_currentMusic.gameObject);
            }

            _currentMusic = CreateMusicSource();
            _currentMusic.Setup(clip, _musicVolume * _masterVolume, 1f, loop: true, spatial: false);
            _currentMusic.Play();
            _currentMusicTrack = track;
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, MusicTrack newTrack)
        {
            _crossfadeMusic = CreateMusicSource();
            _crossfadeMusic.Setup(newClip, 0f, 1f, loop: true, spatial: false);
            _crossfadeMusic.Play();

            float elapsed = 0f;
            float startVolume = _currentMusic != null ? _currentMusic.AudioSource.volume : 0f;
            float targetVolume = _musicVolume * _masterVolume;

            while (elapsed < _musicCrossfadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _musicCrossfadeTime;

                if (_currentMusic != null)
                {
                    _currentMusic.AudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                }

                _crossfadeMusic.AudioSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            if (_currentMusic != null)
            {
                Destroy(_currentMusic.gameObject);
            }

            _currentMusic = _crossfadeMusic;
            _crossfadeMusic = null;
            _currentMusicTrack = newTrack;
        }

        public void StopMusic(bool fade = true)
        {
            if (_currentMusic == null) return;

            if (fade)
            {
                _currentMusic.FadeTo(0f, _musicCrossfadeTime, () =>
                {
                    if (_currentMusic != null)
                    {
                        Destroy(_currentMusic.gameObject);
                        _currentMusic = null;
                    }
                });
            }
            else
            {
                Destroy(_currentMusic.gameObject);
                _currentMusic = null;
            }

            _currentMusicTrack = MusicTrack.None;
        }

        public void PauseMusic()
        {
            _currentMusic?.AudioSource.Pause();
        }

        public void ResumeMusic()
        {
            _currentMusic?.AudioSource.UnPause();
        }

        private ManagedAudioSource CreateMusicSource()
        {
            GameObject go = new GameObject($"Music_{_currentMusicTrack}");
            go.transform.SetParent(_musicContainer);
            return go.AddComponent<ManagedAudioSource>();
        }

        private void UpdateMusicVolume()
        {
            if (_currentMusic != null)
            {
                _currentMusic.AudioSource.volume = _musicVolume * _masterVolume;
            }
        }

        #endregion

        #region Sound Effects

        public static void Play(SoundType soundType, Vector3? position = null)
        {
            EventBus.Push(new SoundPlayEvent(soundType, position), EventBus.EventRegion.GLOBAL);
        }
        
        public void PlaySound(SoundPlayEvent e) => PlaySound(e.SoundType, e.Position);

        public void PlaySound(SoundType type, Vector3? position = null)
        {
            if (!_sfxDict.TryGetValue(type, out SoundGroup group))
            {
                Debug.LogWarning($"Sound type {type} not found!");
                return;
            }

            SoundClip soundClip = group.GetRandomClip();
            if (soundClip == null || soundClip.Clip == null) return;

            var source = _sfxPool.Get();
            
            bool is3D = position.HasValue;
            if (is3D)
            {
                source.transform.position = position.Value;
            }
            else
            {
                source.transform.localPosition = Vector3.zero;
            }

            float finalVolume = soundClip.GetRandomVolume() * group.GroupVolume * _sfxVolume * _masterVolume;
            float finalPitch = soundClip.GetRandomPitch();

            source.Setup(soundClip.Clip, finalVolume, finalPitch, loop: false, spatial: is3D);
            source.Play();
        }

        public void StopAllSounds()
        {
            _sfxPool.StopAll();
        }

        #endregion

        #region Ambient
        
        public static void Play(AmbientType ambientType, Transform anchor = null)
        {
            EventBus.Push(new AmbientPlayEvent(ambientType, anchor), EventBus.EventRegion.GLOBAL);
        }
        
        public static void Stop(AmbientType ambientType, bool fade = true)
        {
            EventBus.Push(new AmbientStopEvent(ambientType, fade), EventBus.EventRegion.GLOBAL);
        }
        
        public void PlayAmbient(AmbientPlayEvent e) => PlayAmbient(e.AmbientType, e.Anchor);

        public void PlayAmbient(AmbientType type, Transform anchor = null)
        {
            if (!_ambientDict.TryGetValue(type, out AmbientSound ambient))
            {
                Debug.LogWarning($"Ambient type {type} not found!");
                return;
            }
            ambient.Volume *= _ambientVolume * _masterVolume;
            _ambientManager.PlayAmbient(ambient);
        }
        
        public void StopAmbient(AmbientStopEvent e) => StopAmbient(e.AmbientType, e.Fade);

        public void StopAmbient(AmbientType type, bool fade = true)
        {
            if (type == AmbientType.All)
            {
                StopAllAmbients(fade);
                return;           
            }
            _ambientManager.StopAmbient(type, fade ? 1f : 0f);
        }

        public void StopAllAmbients(bool fade = true)
        {
            _ambientManager.StopAll(fade ? 1f : 0f);
        }

        public void UpdateAmbientPosition(AmbientType type, Vector3 position)
        {
            _ambientManager.UpdatePosition(type, position);
        }

        #endregion

        private void UpdateAllVolumes()
        {
            UpdateMusicVolume();
        }

        [Serializable]
        public class MusicTrackData
        {
            public MusicTrack Track;
            public AudioClip Clip;
        }

        [Serializable]
        public class SoundTypeData
        {
            public SoundType Type;
            public SoundGroup Group;
        }

        
    }
}
