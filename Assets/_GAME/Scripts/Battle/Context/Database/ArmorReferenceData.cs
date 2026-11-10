using _GAME.Scripts.Battle.Armors;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "ArmorReference_", menuName = "Scriptable/DB/Armor/Reference", order = 1)]
    public class ArmorReferenceData : ScriptableObject
    {
        [SerializeField] private ArmorData _data;
        // Placeholder: armor has no visual/behaviour prefab yet
        [SerializeField] private GameObject _prefab;

        public ArmorData Data => _data;
        public GameObject Prefab => _prefab;
    }
}
