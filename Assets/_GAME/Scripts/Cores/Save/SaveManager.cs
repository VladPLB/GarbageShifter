using System;
using System.Collections.Generic;
using _GAME.Settings.Save;
using UnityEngine;

namespace _GAME.Scripts.Save
{
    public class SaveManager: MonoBehaviour, IRuntimeSetup
    {
        [Serializable]
        private class SaveWrapper
        {
            public List<SaveEntry> Entries;
        }

        [Serializable]
        private class SaveEntry
        {
            public string Type;
            public string Json;
        }
        
        private const string PREF_KEY = "GlobalSave";
        
        [SerializeField] private float _autoSaveDelay = 1f;
        
        private Dictionary<Type, ISaveData> _dataDictionary;

        private float _autoSaveTime = 0;
        private bool _changed = false;
        private void Awake()
        {
            Core.Registry(this, typeof(Settings));
        }

        public void RuntimeSetup()
        {
            _dataDictionary = new Dictionary<Type, ISaveData>();
            SetupDefaults();
            Load();
            _autoSaveTime = Time.time + _autoSaveDelay;
            _changed = false;
        }
        
        private void RegisterData(ISaveData data)
        {
            var type = data.GetType();
            if (_dataDictionary.ContainsKey(type))
            {
                Debug.LogWarning($"[SaveManager] Duplicate data type '{type.Name}' ignored.");
                return;
            }
            _dataDictionary[type] = data;
            data.OnDataChanged += DataChangedHandler;
        }

        private void DataChangedHandler(bool force)
        {
            if (force)
            {
                Save();
            }
            else
            {
                _changed = true;
            }
        }

        private void Update()
        {
            if (Time.time >= _autoSaveTime)
            {
                Save();
            }
        }

        private void Save()
        {
            var wrapper = new SaveWrapper { Entries = new List<SaveEntry>() };
            foreach (var kvp in _dataDictionary)
            {
                string json = JsonUtility.ToJson(kvp.Value);
                string typeString = kvp.Key.AssemblyQualifiedName;
                wrapper.Entries.Add(new SaveEntry { Type = typeString, Json = json });
            }

            string globalJson = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PREF_KEY, globalJson);
            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] Saved all data under key '{PREF_KEY}'");
            _autoSaveTime = Time.time + _autoSaveDelay;
            _changed = false;
        }
        
        private void Load()
        {
            if (!PlayerPrefs.HasKey(PREF_KEY)) return;
            string globalJson = PlayerPrefs.GetString(PREF_KEY);
            try
            {
                var wrapper = JsonUtility.FromJson<SaveWrapper>(globalJson);
                foreach (var entry in wrapper.Entries)
                {
                    var type = Type.GetType(entry.Type);
                    if (type != null && _dataDictionary.ContainsKey(type))
                    {
                        _dataDictionary[type] = JsonUtility.FromJson(entry.Json, type) as ISaveData;
                    }
                }
                Debug.Log($"[SaveManager] Loaded all data from key '{PREF_KEY}'");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load data: {e.Message}");
            }
        }
        
        private void SetupDefaults()
        {
            var settings = Core.Get<Settings>();
            var defaultDataObjects = settings.DefaultSaveData.GetDatas();
            foreach (var obj in defaultDataObjects)
            {
                RegisterData(obj);
            }
        }
        
        public T GetData<T>() where T : ISaveData, new()
        {
            var type = typeof(T);
            if (!_dataDictionary.TryGetValue(type, out var data))
            {
                var instance = new T();
                _dataDictionary[type] = instance;
                instance.OnDataChanged += DataChangedHandler;
                DataChangedHandler(false);
                return instance;
            }
            return (T)data;
        }


        
    }
}