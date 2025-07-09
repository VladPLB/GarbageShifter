using System;
using _GAME.Settings.Save;

namespace _GAME.Scripts.Cores.Save.SavesConfigs
{
    [Serializable]
    public class ProgressData : ISaveData
    {
        public bool Forced => true;
        
        public int Level;

        public event Action<bool> OnDataChanged;

        public void NextLevel()
        {
            Level++;
            OnDataChanged?.Invoke(Forced);
        }
    }
}