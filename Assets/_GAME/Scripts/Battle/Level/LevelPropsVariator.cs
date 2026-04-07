using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelPropsVariator: MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _props;

        private void Start()
        {
            foreach (var p in _props)
            {
                p.gameObject.SetActive(Random.Range(0, 100) <60);
            }
        }
    }
}