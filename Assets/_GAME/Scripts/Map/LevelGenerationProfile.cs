using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Context;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace _GAME.Scripts.Map
{
    [System.Serializable]
    public class LevelTypeWeight
    {
        public MapManager.LevelType levelType;
        public float weight;
    }

    [CreateAssetMenu(menuName = "Scriptable/Level/Generation Profile", fileName = "LevelGenerationProfile")]
    public class LevelGenerationProfile : ScriptableObject
    {
        
        [SerializeField] 
        private int _locationsPerZone = 10;
        [SerializeField] private List<string> _zoneNames;
        [SerializeField] private List<SkyboxSettings> _skyboxSettings;
        [SerializeField] private List<MapManager.LocationType> _locationTypes;
        [SerializeField] 
        private int2 _levelsPerLocationRange;
        [SerializeField]
        private float _zSpacing = 5f;
        [SerializeField]
        private float _xJitter = 1.5f;
        
        [SerializeField]
        private List<LevelTypeWeight> _levelTypeWeights;

        private Random _random;

        public int LocationsPerZone => _locationsPerZone;
        public List<MapManager.LocationType> LocationTypes => _locationTypes.ToList();
        public float ZSpacing => _zSpacing;
        public float XJitter => _xJitter;
        
        public int GetLevelsCount(int location)
        {
            var rand = new Random(location * 123);
            return rand.Next(_levelsPerLocationRange.x, _levelsPerLocationRange.y);
        }
        
        public string GetRandomName(int zone)
        {
            var rand = new Random(zone * 1234);
            return $"{_zoneNames[rand.Next(_zoneNames.Count())]} {rand.Next(1,30).ToRoman()}";
        }
        
        public SkyboxSettings GetRandomSkyBox(int zone)
        {
            var rand = new Random(zone * 4321);
            return _skyboxSettings[rand.Next(_skyboxSettings.Count())];
        }

        public MapManager.LevelType GetRandomLevelType(int level)
        {
            var rand = new Random(level * 354);
            float totalWeight = 0f;
            foreach (var entry in _levelTypeWeights)
                totalWeight += entry.weight;

            float roll = (float)(rand.NextDouble() * totalWeight);
            float cumulative = 0f;

            foreach (var entry in _levelTypeWeights)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                    return entry.levelType;
            }

            return MapManager.LevelType.Default;
        }
    }
}