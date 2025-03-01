using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private List<TutorialStepBase> _steps = new();

        private async void Start()
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                _steps[i].Play();
                await UniTask.WaitWhile(()=>!_steps[i].IsComplete);
            }
        }
    }
}