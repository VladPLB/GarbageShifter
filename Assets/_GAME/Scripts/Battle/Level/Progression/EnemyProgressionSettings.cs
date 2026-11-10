using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Progression
{
    [CreateAssetMenu(fileName = "EnemyProgressionSettings", menuName = "Scriptable/Battle/EnemyProgression", order = 1)]
    public class EnemyProgressionSettings : ScriptableObject
    {
        [Header("Growth by location number")]
        [Tooltip("Location where a fully upgraded player is expected; growth flattens after it")]
        [SerializeField, Min(1)] private int _softCapLocation = 500;
        [SerializeField, Min(1f)] private float _healthAtSoftCap = 4f;
        [SerializeField, Min(1f)] private float _damageAtSoftCap = 3f;
        [Tooltip("<1 - faster growth on early locations, >1 - slower")]
        [SerializeField, Min(0.1f)] private float _growthPower = 0.85f;
        [Tooltip("Growth after the soft cap: part of the soft cap growth gained per each next soft cap length")]
        [SerializeField, Min(0f)] private float _tailGrowth = 0.1f;

        [Header("Rollback after every N-th location")]
        [SerializeField, Min(1)] private int _rollbackPeriod = 10;
        [Tooltip("How many locations of growth are rolled back at the start of each period")]
        [SerializeField, Min(0f)] private float _rollbackLocations = 4f;

        [Header("Adaptive pressure (multiplier saved in progress)")]
        [SerializeField, Min(0f)] private float _pressureOnLocationClear = 0.01f;
        [SerializeField, Min(0f)] private float _pressureOnDefeat = 0.005f;
        [Tooltip("Each previous defeat in a row adds this part of the base defeat penalty")]
        [SerializeField, Min(0f)] private float _defeatStreakGrowth = 1f;
        [SerializeField, Min(0f)] private float _pressureOnDefeatMax = 0.04f;
        [SerializeField] private float _pressureMin = 0.65f;
        [SerializeField] private float _pressureMax = 1.35f;

        public float PressureOnLocationClear => _pressureOnLocationClear;
        // defeatStreak = defeats in a row before this one
        public float GetDefeatPenalty(int defeatStreak)
        {
            var penalty = _pressureOnDefeat * (1f + _defeatStreakGrowth * Mathf.Max(0, defeatStreak));
            return Mathf.Min(penalty, Mathf.Max(_pressureOnDefeat, _pressureOnDefeatMax));
        }

        public float MinPressure => _pressureMin;

        public float ClampPressure(float pressure) => Mathf.Clamp(pressure, _pressureMin, _pressureMax);

        public float GetHealthMultiplier(int location) => 1f + (_healthAtSoftCap - 1f) * GetGrowth(location);
        public float GetDamageMultiplier(int location) => 1f + (_damageAtSoftCap - 1f) * GetGrowth(location);

        // Sawtooth: inside each period growth is slightly steeper, then rolls back at the start of the next one,
        // so the last location of a period always matches the real location number
        public float GetEffectiveLocation(int location)
        {
            if (_rollbackPeriod <= 1 || _rollbackLocations <= 0f)
                return location;

            var period = location / _rollbackPeriod;
            var index = location % _rollbackPeriod;
            var step = (_rollbackPeriod - 1f + _rollbackLocations) / (_rollbackPeriod - 1f);
            return Mathf.Max(0f, period * _rollbackPeriod - _rollbackLocations + index * step);
        }

        private float GetGrowth(int location)
        {
            var t = GetEffectiveLocation(location) / _softCapLocation;
            return t <= 1f
                ? Mathf.Pow(t, _growthPower)
                : 1f + (t - 1f) * _tailGrowth;
        }
    }
}
