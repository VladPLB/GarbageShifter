using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapManager : MonoBehaviour
    {
        public enum LocationType { RoundStation, MedicalStation, ConstructStation, AsteroidStation, BigShip, MinerShip, DroidShip, AsteroidShip, AsteroidMine }
        public enum LevelType { Default, Tutorial, Rage, Elite, Boss }
        
        [SerializeField] private LevelGenerationProfile _generationProfile;
        [SerializeField] private List<LevelZoneConfig> _prepareZones;

        public (int zoneIndex, int locationIndex, int levelIndex) GetInfo(int level)
        {
            int count = 0;
            for (int z = 0; z < _prepareZones.Count; z++)
            {
                var zone = _prepareZones[z].Data;
                for (int l = 0; l < zone.Locations.Count; l++)
                {
                    var location = zone.Locations[l];
                    for (int i = 0; i < location.levels.Count; i++)
                    {
                        if (count == level)
                            return (z, l, i);
                        count++;
                    }
                }
            }
            
            return GenerateLazyZone(count, _prepareZones.Count, level);
        }
        
        public (int zoneIndex, LocationType locationType, LevelType levelType) GetLevelInfo(int level)
        {
            int count = 0;
            for (int z = 0; z < _prepareZones.Count; z++)
            {
                var zone = _prepareZones[z].Data;
                for (int l = 0; l < zone.Locations.Count; l++)
                {
                    var location = zone.Locations[l];
                    for (int i = 0; i < location.levels.Count; i++)
                    {
                        if (count == level)
                            return (z, location.type, location.levels[i]);
                        count++;
                    }
                }
            }
            var newZoneInfo = GenerateLazyZone(count, _prepareZones.Count, level);
            var newZone = GetZone(newZoneInfo.zoneIndex);
            
            return (newZoneInfo.zoneIndex, newZone.Locations[newZoneInfo.locationIndex].type, newZone.Locations[newZoneInfo.locationIndex].levels[newZoneInfo.levelIndex]);
        }

        private void Start()
        {
            Core.Registry(this);
        }

        public LevelZoneData GetZone(int zoneIndex)
        {
            bool isPrepared = zoneIndex < _prepareZones.Count;
            var zone = isPrepared ? _prepareZones[zoneIndex].Data : GenerateZoneData(zoneIndex);
            if(isPrepared)
            {
                var rand = new System.Random(zoneIndex * 7919);
                for (int l = 0; l < zone.Locations.Count; l++)
                {
                    if (zone.Locations[l].uiPosition == Vector3.zero)
                    {
                        zone.Locations[l].uiPosition =
                            new Vector3((float)(rand.NextDouble() * 2 - 1) * _generationProfile.XJitter, 0,
                                l * -_generationProfile.ZSpacing);
                    }
                }
            }
            return zone;
        }
        
        private (int zoneIndex, int locationIndex, int levelIndex) GenerateLazyZone(int count, int zoneIndex ,int level)
        {
            for (int z = zoneIndex; z < 10000; z++)
            {
                for (int l = 0; l < _generationProfile.LocationsPerZone; l++)
                {
                    int levelCount = _generationProfile.GetLevelsCount(zoneIndex * l);
                    for (int i = 0; i < levelCount; i++)
                    {
                        if (count == level)
                            return (z, l, i);
                        count++;
                    }
                }
            }

            return (-1, -1, -1);
        }

        private LevelZoneData GenerateZoneData(int zoneIndex)
        {
            var rand = new System.Random(zoneIndex * 7919);
            var availableTypes = _generationProfile.LocationTypes;
            var locations = new List<LevelLocation>();
            for (int l = 0; l < _generationProfile.LocationsPerZone; l++)
            {
                int levelCount = _generationProfile.GetLevelsCount(zoneIndex * l);
                var locType = availableTypes.Count > 0 ? availableTypes.PopRandom(rand) : LocationType.BigShip;

                var location = new LevelLocation
                {
                    type = locType,
                    uiPosition = new Vector3((float)(rand.NextDouble() * 2 - 1) * _generationProfile.XJitter, 0, l * -_generationProfile.ZSpacing),
                    levels = new List<LevelType>()
                };

                for (int i = 0; i < levelCount; i++)
                {
                    var isLast = i == levelCount - 1;
                    var lType = isLast ? LevelType.Boss : _generationProfile.GetRandomLevelType(i*l*zoneIndex);

                    location.levels.Add(lType);
                }
                
                locations.Add(location);
            }
            var zone = new LevelZoneData(_generationProfile.GetRandomName(zoneIndex), locations, _generationProfile.GetRandomSkyBox(zoneIndex));
            return zone;
        }
    }
}