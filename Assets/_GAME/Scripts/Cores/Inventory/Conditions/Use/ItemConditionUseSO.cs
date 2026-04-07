using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    public abstract class ItemConditionUseSO : ScriptableObject
    {
        public virtual bool CanUse() => false;
    }

}