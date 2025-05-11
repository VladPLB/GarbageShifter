using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using _GAME.Scripts.UI;
using UnityEngine;

namespace _GAME.Scripts
{
    public class UIManager : MonoBehaviour, IRuntimeSetup
    {
        [Serializable]
        public class UILayer
        {
            public string name;
            public Transform root;
        }

        [Header("Canvas Setup")]
        [SerializeField] private List<UILayer> layers = new();
        [SerializeField] private GameObject interactionBlockerPrefab;

        [Header("Window Prefabs")]
        [SerializeField]
        private List<UIWindow> windowPrefabs = new();

        private Dictionary<string, Transform> _layerMap = new();
        private Dictionary<Type, GameObject> _activeWindows = new();
        private Stack<GameObject> _windowStack = new();
        private Dictionary<Type, GameObject> _windowPool = new();
        private Dictionary<Type, GameObject> _prefabMap = new();
        private GameObject _interactionBlocker;

        private void Awake()
        {
            Core.Registry(this);
        }

        public async void RuntimeSetup()
        {
            InitializeLayerMap();
            InitializePrefabMap();
            EventBus.Push(new KeyEvent("UILoaded"), EventBus.EventRegion.GLOBAL);
        }

        private void InitializeLayerMap()
        {
            foreach (var layer in layers)
            {
                if (!_layerMap.ContainsKey(layer.name))
                    _layerMap.Add(layer.name, layer.root);
            }
        }

        private void InitializePrefabMap()
        {
            foreach (var prefab in windowPrefabs)
            {
                var type = prefab.GetType();
                if (!_prefabMap.ContainsKey(type))
                    _prefabMap.Add(type, prefab.gameObject);
            }
        }
        
        private T OpenWindow<T>(GameObject prefab, string layerName, bool blockInteraction) where T : UIWindow
        {
            if (!_layerMap.TryGetValue(layerName, out var parent))
            {
                Debug.LogWarning($"UI Layer '{layerName}' not found.");
                return null;
            }

            GameObject instance;
            var type = typeof(T);

            if (_windowPool.TryGetValue(type, out var pooled) && pooled != null)
            {
                instance = pooled;
                instance.transform.SetParent(parent, false);
            }
            else
            {
                instance = Instantiate(prefab, parent);
            }

            T component = instance.GetComponent<T>();

            if (component == null)
            {
                Debug.LogWarning("Prefab does not contain component of type " + typeof(T));
                Destroy(instance);
                return null;
            }

            component.SetManager(this);
            component.OnOpen();

            _activeWindows[type] = instance;
            _windowStack.Push(instance);

            if (blockInteraction)
                ShowInteractionBlocker(parent);

            return component;
        }

        public T OpenWindow<T>(string layerName = "Default", bool blockInteraction = false) where T : UIWindow
        {
            if (!_prefabMap.TryGetValue(typeof(T), out var prefab))
            {
                Debug.LogError("Prefab for window type " + typeof(T).Name + " not found in UIManager config.");
                return null;
            }

            var type = typeof(T);
            if (_activeWindows.TryGetValue(type, out var existingWindow))
            {
                existingWindow.transform.SetAsLastSibling();
                var tempStack = new Stack<GameObject>(_windowStack.Count);
                while (_windowStack.Count > 0)
                {
                    var top = _windowStack.Pop();
                    if (top == existingWindow) break;
                    tempStack.Push(top);
                }

                _windowStack.Push(existingWindow);
                while (tempStack.Count > 0)
                    _windowStack.Push(tempStack.Pop());

                return existingWindow.GetComponent<T>();
            }

            return OpenWindow<T>(prefab, layerName, blockInteraction);
        }
        
        public void CloseWindow(Type windowType)
        {
            if (_activeWindows.TryGetValue(windowType, out var window))
            {
                if (_windowStack.Contains(window))
                {
                    var tempStack = new Stack<GameObject>(_windowStack.Count);
                    while (_windowStack.Count > 0)
                    {
                        var top = _windowStack.Pop();
                        if (top == window) break;
                        tempStack.Push(top);
                    }

                    while (tempStack.Count > 0)
                        _windowStack.Push(tempStack.Pop());
                }

                var ui = window.GetComponent<UIWindow>();
                if (ui != null) ui.OnClose();
                else window.SetActive(false);

                _activeWindows.Remove(windowType);
                if (!_windowPool.ContainsKey(windowType))
                    _windowPool.Add(windowType, window);
            }

            if (_windowStack.Count == 0)
                HideInteractionBlocker();
        }

        public void CloseTopWindow()
        {
            if (_windowStack.Count > 0)
            {
                var window = _windowStack.Pop();
                var ui = window.GetComponent<UIWindow>();
                var type = ui.GetType();

                if (ui != null) ui.OnClose();
                else window.SetActive(false);

                if (_activeWindows.ContainsKey(type))
                    _activeWindows.Remove(type);
                if (!_windowPool.ContainsKey(type))
                    _windowPool.Add(type, window);
            }

            if (_windowStack.Count == 0)
                HideInteractionBlocker();
        }

        public void CloseAllWindows()
        {
            foreach (var window in _activeWindows.Values)
            {
                var ui = window.GetComponent<UIWindow>();
                var type = ui.GetType();

                if (ui != null) ui.OnClose();
                else window.SetActive(false);

                if (!_windowPool.ContainsKey(type))
                    _windowPool.Add(type, window);
            }

            _activeWindows.Clear();
            _windowStack.Clear();
            HideInteractionBlocker();
        }

        public void ClearWindowPool()
        {
            foreach (var kvp in _windowPool)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }

            _windowPool.Clear();
        }

        public T GetTopWindow<T>() where T : UIWindow
        {
            return _windowStack.Count > 0 ? _windowStack.Peek().GetComponent<T>() : null;
        }

        public bool HasWindow<T>() where T : UIWindow
        {
            return _activeWindows.ContainsKey(typeof(T));
        }

        public T GetWindow<T>() where T : UIWindow
        {
            return _activeWindows.TryGetValue(typeof(T), out var go) ? go.GetComponent<T>() : null;
        }

        public void BringToFront<T>() where T : UIWindow
        {
            if (_activeWindows.TryGetValue(typeof(T), out var window))
            {
                window.transform.SetAsLastSibling();

                var tempStack = new Stack<GameObject>(_windowStack.Count);
                while (_windowStack.Count > 0)
                {
                    var top = _windowStack.Pop();
                    if (top == window) break;
                    tempStack.Push(top);
                }

                _windowStack.Push(window);
                while (tempStack.Count > 0)
                    _windowStack.Push(tempStack.Pop());
            }
        }

        private void ShowInteractionBlocker(Transform parent)
        {
            if (_interactionBlocker == null && interactionBlockerPrefab != null)
            {
                _interactionBlocker = Instantiate(interactionBlockerPrefab, parent);
                _interactionBlocker.transform.SetAsFirstSibling();
            }
            else if (_interactionBlocker != null)
            {
                _interactionBlocker.SetActive(true);
            }
        }

        private void HideInteractionBlocker()
        {
            if (_interactionBlocker != null)
                _interactionBlocker.SetActive(false);
        }
    }
}