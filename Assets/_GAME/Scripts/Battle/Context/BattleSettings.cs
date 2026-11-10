using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Level.Progression;
using _GAME.Scripts.Common;
using _GAME.Scripts.Map;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "BattleSettings", menuName = "Scriptable/Battle/Settings", order = 0)]
    public class BattleSettings : ScriptableObject
    {
        [SerializeField]
        protected List<LightSettings> _lights = new();
        [SerializeField] private EnemyProgressionSettings _enemyProgression;
        [SerializeField] private PlayerProgressionSettings _playerProgression;
        [SerializeField] private LootProgressionSettings _lootProgression;

        public EnemyProgressionSettings EnemyProgression => _enemyProgression;
        public PlayerProgressionSettings PlayerProgression => _playerProgression;
        public LootProgressionSettings LootProgression => _lootProgression;
        
        public LightSettings GetLightSettings(MapManager.LocationType location)
        {
           var l = _lights.FirstOrDefault(l => l.Location == location);
           return l ?? _lights[0];
        }
    }

    [Serializable]
    public class LightSettings
    {
        public MapManager.LocationType Location;
        public Color Color;
        public float Intensity;
    }
}