using _GAME.Scripts.Cores.Save.SavesConfigs;

namespace _GAME.Scripts.Battle.Level.Progression
{
    public readonly struct EnemyScaling
    {
        public readonly float Health;
        public readonly float Damage;

        public static EnemyScaling Default => new(1f, 1f);

        public EnemyScaling(float health, float damage)
        {
            Health = health;
            Damage = damage;
        }
    }

    // Enemy difficulty for the current level: location based growth * adaptive pressure
    public class EnemyProgression
    {
        private readonly EnemyProgressionSettings _settings;
        private readonly ProgressData _progressData;
        private readonly bool _isLastLevelInLocation;
        private bool _isResolved;

        public int Location { get; }
        public bool IsLastLevelInLocation => _isLastLevelInLocation;
        // Level is played with the one-level pressure buff (minimal pressure)
        public bool IsBuffed { get; }
        public EnemyScaling Scaling { get; }

        public float Pressure => _settings != null
            ? _settings.ClampPressure(1f + _progressData.EnemyPressureOffset)
            : 1f;

        public EnemyProgression(EnemyProgressionSettings settings, ProgressData progressData, int location, bool isLastLevelInLocation)
        {
            _settings = settings;
            _progressData = progressData;
            _isLastLevelInLocation = isLastLevelInLocation;
            Location = location;

            // Buff is consumed on level start; the saved pressure itself is not changed by it
            if (_progressData.EnemyPressureBuff)
            {
                IsBuffed = true;
                _progressData.SetEnemyPressureBuff(false);
            }

            var pressure = _settings == null ? 1f : IsBuffed ? _settings.MinPressure : Pressure;
            Scaling = _settings == null
                ? EnemyScaling.Default
                : new EnemyScaling(_settings.GetHealthMultiplier(location) * pressure, _settings.GetDamageMultiplier(location) * pressure);
        }

        public void OnLevelCompleted()
        {
            if (_isResolved || _settings == null)
                return;

            _isResolved = true;
            // any won level breaks the defeat streak, pressure grows only on location clear
            ChangePressure(_isLastLevelInLocation ? _settings.PressureOnLocationClear : 0f, 0);
        }

        public void OnDefeat()
        {
            if (_isResolved || _settings == null)
                return;

            _isResolved = true;
            var streak = _progressData.DefeatStreak;
            ChangePressure(-_settings.GetDefeatPenalty(streak), streak + 1);
        }

        private void ChangePressure(float delta, int defeatStreak)
        {
            var pressure = _settings.ClampPressure(Pressure + delta);
            _progressData.SetEnemyPressure(pressure - 1f, defeatStreak);
        }
    }
}
