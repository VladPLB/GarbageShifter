using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    public abstract class ItemUsePipelineSO : ScriptableObject
    {
        public virtual bool Use(ItemInfo id) => false;
    }
}