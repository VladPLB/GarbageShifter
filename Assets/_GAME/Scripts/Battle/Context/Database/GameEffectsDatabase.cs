using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "GameEffectsDatabase", menuName = "Scriptable/DB/Effects/GameEffects", order = 0)]
    public class GameEffectsDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<GameEffect> _prefabs = new();
        private Dictionary<GameEffectType, GameEffect> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public GameEffect GetPrefab(GameEffectType type)
        {
            return _prefabsByType[type];
        }
    }
}