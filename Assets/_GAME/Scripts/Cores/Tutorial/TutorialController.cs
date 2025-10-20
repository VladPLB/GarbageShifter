using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private int _tutorialStep;
        [SerializeField] private List<TutorialStepBase> _steps = new();
        [SerializeField] private int _currentStep = 0;

        private TutorialEntryData _tutorialEntryData;
        private SaveManager _saveManager;
        public void Initialize()
        {
            _saveManager = Core.Get<SaveManager>();
            if(_saveManager == null)
                return;
            
            _tutorialEntryData = _saveManager.GetData<TutorialEntryData>();
            if (_tutorialEntryData.TutorialStep < _tutorialStep)
            {
                _tutorialEntryData.OnDataChanged += Play;
            }
            else if (_tutorialEntryData.TutorialStep == _tutorialStep)
            {
                Play(false);
            }
        }

        private void OnDestroy()
        {
            if(_tutorialEntryData!=null)
            {
                _tutorialEntryData.OnDataChanged -= Play;
            }
        }

        private async void Play(bool forced)
        {
            foreach (var t in _steps)
            {
                t.Play();
                _currentStep++;
                if (t.IsComplete)
                {
                    continue;
                }
                await UniTask.WaitWhile(()=>!t.IsComplete);
            }
            
            _tutorialEntryData.NextStep();
        }
    }
}