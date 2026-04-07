using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    public abstract class ItemConditionCraftSO : ScriptableObject
    {
        public virtual bool CanCraft() => false;

    }
}