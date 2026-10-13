using System;
using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/ByPlace", fileName = "ItemUsePipelineByPlace_")]
    public class ItemUsePipelineByPlace : ItemUsePipelineSO
    {
        [Serializable]
        private class PlacePipelines
        {
            public List<PlaceTarget> Targets = new();
            public List<ItemUsePipelineSO> Pipelines = new();
        }

        [SerializeField] private List<PlacePipelines> _pipelines = new();

        public override bool IsAvailable(ItemInfo info) => false;

        public override void CollectUseInfos(ItemInfo info, List<ItemUseInfo> infos)
        {
            base.CollectUseInfos(info, infos);
            foreach (var entry in _pipelines)
            {
                foreach (var pipeline in entry.Pipelines)
                {
                    if (pipeline != null && pipeline != this)
                        pipeline.CollectUseInfos(info, infos);
                }
            }
        }

        public override void CollectActions(ItemInfo info, List<ItemUsePipelineSO> actions)
        {
            var place = Core.Get<InventoryManager>().PlaceTarget;
            foreach (var entry in _pipelines)
            {
                if (!entry.Targets.Contains(place))
                    continue;

                foreach (var pipeline in entry.Pipelines)
                {
                    if (pipeline != null && pipeline != this)
                        pipeline.CollectActions(info, actions);
                }
            }
        }
    }
}
