using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public class UseDescriptionInfoSection : ItemUseInfoSection<ItemUseDescriptionInfo>
    {
        [SerializeField] private TextMeshProUGUI _descriptionLabel;

        protected override void Show(ItemInfoRequest request, List<ItemUseDescriptionInfo> infos)
        {
            _descriptionLabel.text = string.Join("\n", infos.Select(i => i.Text));
        }
    }
}
