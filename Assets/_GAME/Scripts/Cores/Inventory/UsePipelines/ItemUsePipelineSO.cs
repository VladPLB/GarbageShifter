using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    public abstract class ItemUsePipelineSO : ScriptableObject
    {
        [SerializeField] private ItemUseButtonView _buttonView = new();
        [TextArea(2, 6)]
        [SerializeField] private string _useDescription;

        public virtual ItemUseButtonView GetButtonView(ItemInfo info) => _buttonView;
        public virtual bool IsAvailable(ItemInfo info) => true;
        public virtual bool CanExecute(ItemInfo info) => true;
        public virtual bool Use(ItemInfo info, ItemUseResult result) => false;

        public virtual void CollectActions(ItemInfo info, List<ItemUsePipelineSO> actions)
        {
            if (IsAvailable(info) && !actions.Contains(this))
                actions.Add(this);
        }

        public virtual void CollectUseInfos(ItemInfo info, List<ItemUseInfo> infos)
        {
            if (!string.IsNullOrEmpty(_useDescription))
                infos.Add(new ItemUseDescriptionInfo(_useDescription));
        }
    }
}
