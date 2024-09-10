using _GAME.Scripts.Battle.Weapons;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "WeaponReference_", menuName = "Scriptable/DB/Weapon/Reference", order = 1)]
    public class WeaponReferenceData : ScriptableObject
    {
        [SerializeField] private WeaponData _data;
        [SerializeField] private Weapon _prefab;

        public WeaponData Data => _data;
        public Weapon Prefab => _prefab;
    }
}