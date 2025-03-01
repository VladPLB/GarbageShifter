using UnityEngine;

namespace _GAME.Scripts.Tutorial
{
    public abstract class TutorialStepBase : MonoBehaviour
    {
        public abstract bool IsComplete { get; }
        public abstract void Play();
    }
}