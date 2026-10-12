using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/UpgradableItemInfo", fileName = "UpgradableItem_")]
    public class UpgradableItemInfo : ItemInfo
    {
        [Header("Upgrade Info")]
        [SerializeField]
        private int _maxLevel;
        [SerializeField] private List<ItemAmountGroup> _upgradePrices;
        
        public int MaxLevel =>  Mathf.Max(0, _maxLevel-1);

        public List<ItemAmount> GetPriceByLevel(int level)
        {
            return _upgradePrices[Mathf.Min(level, _upgradePrices.Count-1)].Items;
        }
    }

}