using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class ItemInfoWindow : ItemInfoWindowBase
    {
        [Header("Buttons")]
        [SerializeField] private RectTransform _buttonsHolder;
        [SerializeField] private ItemInfoButton _buttonPrefab;

        private LocalPool<ItemInfoButton> _pool;
        private readonly List<ItemInfoButton> _activeButtons = new();

        public ItemInfo Item => Request?.Item;

        public override void OnOpen()
        {
            _pool ??= new LocalPool<ItemInfoButton>(_buttonPrefab);
            base.OnOpen();
        }

        public override void OnClose()
        {
            ClosePreview();
            ReleaseButtons();
            base.OnClose();
        }

        public override void Setup(ItemInfoRequest request)
        {
            ClosePreview();
            base.Setup(request);
            SetupButtons(request);
        }

        private void SetupButtons(ItemInfoRequest request)
        {
            ReleaseButtons();

            foreach (var option in request.Buttons)
            {
                var button = _pool.Pop();
                button.Setup(option.View, option.Interactable, () => OnButtonClicked(option));
                button.transform.SetParent(_buttonsHolder, false);
                button.transform.localScale = Vector3.one;
                button.gameObject.SetActive(true);
                _activeButtons.Add(button);
            }
        }

        private void OnButtonClicked(ItemInfoButtonOption option)
        {
            if (option.CloseOnClick)
                Close();

            option.Callback?.Invoke();
        }

        private void ClosePreview()
        {
            if (manager != null)
                manager.GetWindow<ItemPreviewWindow>()?.Close();
        }

        private void ReleaseButtons()
        {
            if (_pool == null)
                return;

            foreach (var button in _activeButtons)
            {
                _pool.Push(button);
            }
            _activeButtons.Clear();
        }
    }
}
