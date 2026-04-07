using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    public abstract class ItemConditionShowSO : ScriptableObject
    {
        public virtual bool CanShow()=> false;
    }

}