using System;
using _GAME.Settings.Save;

namespace _GAME.Scripts.Cores.Save.SavesConfigs
{
    [Serializable]
    public class ProgressData : ISaveData
    {
        public bool Forced => true;
        
        public int Level;
        // Adaptive enemy difficulty: pressure = 1 + offset (offset keeps old saves at neutral 0)
        public float EnemyPressureOffset;
        public int DefeatStreak;
        // One-level buff (e.g. rewarded video): next level is played with minimal enemy pressure
        public bool EnemyPressureBuff;
        // Levels played to the end (won or lost), used for ads frequency
        public int LevelsPlayed;

        public event Action<bool> OnDataChanged;

        public void NextLevel()
        {
            Level++;
            OnDataChanged?.Invoke(Forced);
        }

        public void AddLevelPlayed()
        {
            LevelsPlayed++;
            OnDataChanged?.Invoke(Forced);
        }

        public void SetEnemyPressureBuff(bool isActive)
        {
            EnemyPressureBuff = isActive;
            OnDataChanged?.Invoke(Forced);
        }

        public void SetEnemyPressure(float offset, int defeatStreak)
        {
            EnemyPressureOffset = offset;
            DefeatStreak = defeatStreak;
            OnDataChanged?.Invoke(Forced);
        }
    }
}