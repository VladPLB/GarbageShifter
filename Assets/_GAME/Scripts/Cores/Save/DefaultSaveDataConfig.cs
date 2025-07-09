using System;
using System.Collections.Generic;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using UnityEngine;

namespace _GAME.Settings.Save
{
    
    public interface ISaveData
    {
        bool Forced { get; }

        event Action<bool> OnDataChanged;
    }
    [CreateAssetMenu(menuName = "Save/DefaultSaveDataConfig", fileName = "DefaultSaveDataConfig")]
    public class DefaultSaveDataConfig : ScriptableObject
    {
        [SerializeField] private ProgressData _progressData;
        [SerializeField] private TutorialEntryData _tutorialEntryData;
        public List<ISaveData> GetDatas()
        {
            List<ISaveData> _datas = new()
            {
                _progressData.Clone(),
                _tutorialEntryData.Clone()
            };
            
            return _datas;
        }
    }
}