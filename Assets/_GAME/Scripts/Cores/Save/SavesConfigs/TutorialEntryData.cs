using System;
using _GAME.Settings.Save;

namespace _GAME.Scripts.Cores.Save.SavesConfigs
{
    [Serializable]
    public class TutorialEntryData : ISaveData
    {
        public bool Forced => true;
        
        public int TutorialStep = 0;

        public event Action<bool> OnDataChanged;

        public void NextStep()
        {
            TutorialStep++;
            OnDataChanged?.Invoke(Forced);
        }
    }
}