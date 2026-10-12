using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using UnityEngine;
using Object = System.Object;

namespace _GAME.Scripts
{
    public class Core: MonoBehaviour
    {
        private static Core _instance;

        private Dictionary<Type, object> _modules = new Dictionary<Type, object> ();
        private static Core Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Core>();
                }

                return _instance;
            }
        }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            //DontDestroyOnLoad(gameObject);
            InitModules();
        }
        private void InitModules()
        {
            
        }

        private readonly struct PendingRegistration
        {
            public readonly Type Type;
            public readonly object Module;
            public readonly Func<bool> IsReady;

            public PendingRegistration(Type type, object module, Func<bool> isReady)
            {
                Type = type;
                Module = module;
                IsReady = isReady;
            }
        }

        private static readonly List<PendingRegistration> _registryQueue = new();

        public static void Unregistry<T>(T module) where T : class
        {
            if (Instance._modules.ContainsKey(typeof(T)))
            {
                Instance._modules.Remove(typeof(T));
            }
        }

        public static void Registry<T>(T module, params Type[] conditions) where T : class
        {
            var type = typeof(T);

            if (!conditions.IsNullOrEmpty())
            {
                if (!Instance._modules.ContainsKey(type))
                {
                    Instance._modules.Add(type, null);
                }

                var c = conditions.ToArray();
                bool IsReady()
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (!Instance._modules.TryGetValue(c[i], out var dep) || dep == null)
                            return false;
                    }
                    return true;
                }

                _registryQueue.Add(new PendingRegistration(type, module, IsReady));
            }
            else
            {
                RegisterImmediate(type, module);
            }

            ResolveQueue();
        }

        // Registers a module that has no outstanding dependencies: stores it (first registration wins;
        // a later call for the same type with a different instance is dropped, not silently overwritten)
        // and runs its one-time setup.
        private static void RegisterImmediate(Type type, object module)
        {
            if (Instance._modules.TryGetValue(type, out var existing) && existing != null)
            {
                if (!ReferenceEquals(existing, module))
                {
                    Debug.LogWarning($"[Core] Duplicate registration of '{type.Name}' ignored - '{existing}' is already registered, '{module}' was discarded.");
                }
                return;
            }

            Instance._modules[type] = module;

            if (module is MonoBehaviour behaviour and not IReparentIgnored)
            {
                behaviour.transform.SetParent(Instance.transform);
            }

            if (module is IRuntimeSetup item)
            {
                item.RuntimeSetup();
            }
        }

        // Keeps draining the queue until a full pass resolves nothing, so multiple entries that
        // become ready from the same Registry() call are all processed immediately instead of
        // waiting for an unrelated future Registry() call to nudge the queue again.
        private static void ResolveQueue()
        {
            bool resolvedAny;
            do
            {
                resolvedAny = false;
                for (int i = 0; i < _registryQueue.Count; i++)
                {
                    if (!_registryQueue[i].IsReady())
                        continue;

                    var pending = _registryQueue[i];
                    _registryQueue.RemoveAt(i);
                    RegisterImmediate(pending.Type, pending.Module);
                    resolvedAny = true;
                    break;
                }
            } while (resolvedAny);
        }

        public static T Get<T>() where T : class
        {
            if (Instance._modules.ContainsKey(typeof(T)))
            {
                return Instance._modules[typeof(T)] as T;
            }

            return null;
        }
    }
}