using _GAME.Scripts.Battle.Equipment;
using _GAME.Scripts.Inventory;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public class EquipmentStatsInfoSection : ItemInfoSection
    {
        [SerializeField] private GameObject _statsRoot;
        [SerializeField] private TextMeshProUGUI _statsLabel;
        [SerializeField] private GameObject _equippedMark;

        private InventoryManager _inventoryManager;
        private DataBase _dataBase;

        public override void Setup(ItemInfoRequest request)
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _dataBase ??= Core.Get<DataBase>();

            var data = GetEquipmentData(request.Item);

            if (_equippedMark != null)
                _equippedMark.SetActive(data != null && _inventoryManager.CanEquip(request.Item));

            var label = data == null ? null : request.StatsMode switch
            {
                EquipmentStatsMode.Current => data.GetStatsLabel(),
                EquipmentStatsMode.UpgradePreview => data.Level < data.MaxLevel
                    ? data.GetStatsLabelForUpgrade()
                    : data.GetStatsLabel(),
                _ => null,
            };

            _statsRoot.SetActive(!string.IsNullOrEmpty(label));
            _statsLabel.text = label;
        }

        private IEquipmentData GetEquipmentData(ItemInfo item)
        {
            if (item is not UpgradableItemInfo)
                return null;

            var data = _dataBase.GetEquipmentData(item.Id);
            data?.SetLevel(_inventoryManager.GetLevel(item));
            return data;
        }
    }
}
