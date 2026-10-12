using System;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    public class EquipmentInfoButton : MonoBehaviour, IPoolableItem
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        public void Setup(string label, bool interactable, Action clickHandler)
        {
            _label.text = label;
            _button.interactable = interactable;
            _button.SetListener(clickHandler);
        }
    }
}
