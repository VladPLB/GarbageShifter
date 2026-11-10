using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Progression
{
    // Passive player bonuses from the game progress (location number, not level)
    [CreateAssetMenu(fileName = "PlayerProgressionSettings", menuName = "Scriptable/Battle/PlayerProgression", order = 2)]
    public class PlayerProgressionSettings : ScriptableObject
    {
        [Header("Passive health")]
        [Tooltip("Health grows once per this amount of locations")]
        [SerializeField, Min(1)] private int _healthStepLocations = 10;
        [Tooltip("Location where the health bonus reaches its peak")]
        [SerializeField, Min(1)] private int _healthPeakLocation = 1000;
        [SerializeField, Min(0)] private int _healthBonusAtPeak = 1000;
        [Tooltip("1 - equal steps, <1 - bigger steps early, >1 - bigger steps late")]
        [SerializeField, Min(0.1f)] private float _healthGrowthPower = 1f;

        public int GetHealthBonus(int location)
        {
            var steps = Mathf.Min(location, _healthPeakLocation) / _healthStepLocations;
            var maxSteps = Mathf.Max(1, _healthPeakLocation / _healthStepLocations);
            var t = Mathf.Pow((float)steps / maxSteps, _healthGrowthPower);
            return Mathf.RoundToInt(_healthBonusAtPeak * t);
        }
    }
}
