using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Battle.Level;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    [System.Serializable]
    public class LevelZoneData
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private List<LevelLocation> _locations;
        [SerializeField]
        private SkyboxSettings _skyboxSettings;

        public string GetName => _name;
        public List<LevelLocation> Locations => _locations;
        public LevelZoneData(string name, List<LevelLocation> locations, SkyboxSettings skyboxSettings) =>
            (_name, _locations, _skyboxSettings) = (name, locations, skyboxSettings);

        public void Setup()
        {
            _skyboxSettings.Setup();
        }
    }
    
    [System.Serializable]
    public class LevelLocation
    {
        public MapManager.LocationType type;
        public List<MapManager.LevelType> levels;
        public Vector3 uiPosition;
    }

    [CreateAssetMenu(menuName = "Scriptable/Map/Zona", fileName = "LevelZone")]
    public class LevelZoneConfig : ScriptableObject
    {
        [SerializeField]
        private LevelZoneData _data;

        public LevelZoneData Data => _data;
    }
}