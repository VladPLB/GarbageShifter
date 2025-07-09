using _GAME.Scripts.Lobby;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapPathBuilder _mapPathBuilder;

        private LevelZoneData _zoneData;
        private int _locationIndex;
        private int _levelIndex;

        public void Initialize(LevelZoneData zoneData, int currentLocationIndex, int currentLevelIndex)
        {
            _zoneData = zoneData;
            _locationIndex = currentLocationIndex;
            _levelIndex = currentLevelIndex;
            
        }

        public void Show()
        {
            _mapPathBuilder.Init(_zoneData, _locationIndex, _levelIndex);
        }

        public void Hide()
        {
            _mapPathBuilder.Release();
        }
    }
}