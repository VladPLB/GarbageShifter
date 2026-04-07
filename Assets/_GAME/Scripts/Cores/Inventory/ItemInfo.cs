using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/ItemInfo", fileName = "Item_")]
    public class ItemInfo : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [TextArea(2, 6)]
        [SerializeField] private string _description;

        [Header("Visual")]
        [SerializeField] private Sprite _icon;

        [Header("Meta")]
        [SerializeField] private ItemRank _rank = ItemRank.Common;
        [SerializeField] private ItemType _type = ItemType.Other;
        
        [SerializeField] private string _subType;

        [Header("Stack")] [SerializeField] private bool _zeroCountEnabled = false;
        [SerializeField] private int _maxStack = 99;

        [Header("Conditions")]
        [SerializeField] private List<ItemConditionShowSO> _showConditions = new();
        [SerializeField] private List<ItemConditionCraftSO> _craftConditions = new();
        [SerializeField] private List<ItemConditionUseSO> _useConditions = new();

        [Header("Recipe")]
        [SerializeField] private ItemRecipe _recipe = new();

        [Header("Use Pipeline")]
        [SerializeField] private ItemUsePipelineSO _usePipeline;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;

        public ItemRank Rank => _rank;
        public ItemType Type => _type;
        public string SubType => _subType;
        
        public bool ZeroCountEnabled => _zeroCountEnabled;
        public int MaxStack => Mathf.Max(1, _maxStack);

        public IReadOnlyList<ItemConditionShowSO> ShowConditions => _showConditions;
        public IReadOnlyList<ItemConditionCraftSO> CraftConditions => _craftConditions;
        public IReadOnlyList<ItemConditionUseSO> UseConditions => _useConditions;

        public ItemRecipe Recipe => _recipe;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _maxStack = Mathf.Max(1, _maxStack);
        }
#endif
    }

}