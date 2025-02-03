using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    public class FillPlane : MonoBehaviour
    {
        private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");
        
        [SerializeField] private Renderer _renderer;

        private Material _material;
        private float _progress = 0;

        public float Progress
        {
            get => _progress;
            set
            {
                UpdateProgress(value);
            }
        }

        private void UpdateProgress(float value)
        {
            _progress = Mathf.Clamp01(value);
            _material ??= _renderer.material;
            _material.SetFloat(FillAmount, _progress);
        }
    }
}