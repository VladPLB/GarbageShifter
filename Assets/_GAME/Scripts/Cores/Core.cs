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

        private static List<Func<object>> _registryQueue = new();

        public static void Unregistry<T>(T module) where T : class
        {
            if (Instance._modules.ContainsKey(typeof(T)))
            {
                Instance._modules.Remove(typeof(T));
            }
        }
        
        public static void Registry<T>(T module, params Type[] conditions) where T : class
        {
            if (!conditions.IsNullOrEmpty())
            {
                if (!Instance._modules.ContainsKey(typeof(T)))
                {
                    Instance._modules.Add(typeof(T), null);
                }

                var c = conditions.ToArray();
                var m = module;
                var t = typeof(T);

                Func<T> action = () =>
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (!Instance._modules.ContainsKey(c[i]) || Instance._modules[c[i]] == null)
                            return null;
                    }

                    if (Instance._modules[t] == null)
                    {
                        Instance._modules[t] = m;
                    }
                    return m;
                };
                _registryQueue.Add(action);
            }
            else
            {
                if (!Instance._modules.ContainsKey(typeof(T)))
                {
                    Instance._modules.Add(typeof(T), module);
                }

                if (Instance._modules[typeof(T)] == null)
                {
                    Instance._modules[typeof(T)] = module;
                }

                if (module is MonoBehaviour behaviour and not IReparentIgnored)
                {
                    behaviour.transform.SetParent(Instance.transform);
                }
                
                if (module is IRuntimeSetup item)
                {
                    item.RuntimeSetup();
                }
            }

            for (int i = 0; i < _registryQueue.Count; i++)
            {
                var m = _registryQueue[i].Invoke();
                if (m!=null)
                {
                    _registryQueue.RemoveAt(i);
                    Registry(m);
                    return;
                }
            }
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