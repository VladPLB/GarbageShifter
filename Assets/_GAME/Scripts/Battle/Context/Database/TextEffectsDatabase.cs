using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "TextEffectsDatabase", menuName = "Scriptable/DB/Effects/Texts", order = 1)]
    public class TextEffectsDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<TextEffect> _prefabs = new();
        private Dictionary<TextEffectType, TextEffect> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public TextEffect GetPrefab(TextEffectType type)
        {
            return _prefabsByType[type];
        }
    }
}