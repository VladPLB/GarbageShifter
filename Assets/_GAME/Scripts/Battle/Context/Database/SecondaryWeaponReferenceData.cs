using _GAME.Scripts.Battle.Player.SecondaryWeapon;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "SecondaryWeaponReference_", menuName = "Scriptable/DB/SecondaryWeapon/Reference", order = 1)]
    public class SecondaryWeaponReferenceData : ScriptableObject
    {
        [SerializeField] private SecondaryWeaponData _data;
        // Placeholder: the player still uses the secondary weapon placed in Player.prefab
        [SerializeField] private SecondaryWeaponBase _prefab;

        public SecondaryWeaponData Data => _data;
        public SecondaryWeaponBase Prefab => _prefab;
    }
}
