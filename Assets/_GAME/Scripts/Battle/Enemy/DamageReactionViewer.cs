using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Enemy
{
    public class DamageReactionViewer : MonoBehaviour
    {
        private const float DURATION = .05f;
        private const float DELAY_AFTER_ANIMATION= .05f;
        private const float REACTION_FORCE_ADDED = .05f;
        private const float MAX_REACTION_FORCE = 1.5f;
        private const float REACTION_FORCE_HIDE_MULTIPLIER = .1f;

        [SerializeField] private bool _isShowTextDamage;
        [SerializeField] private List<Renderer> _renderers;
        [SerializeField] private Material _reactionMaterial;

        private List<Material> _defaultMaterials = null;
        private bool _animate = false;
        private float _reactionForce = 1f;

        private void OnEnable()
        {
            _renderers = _renderers.Where(r => r != null).ToList();
            _reactionForce = 1f;
            _animate = false;
            _defaultMaterials = _renderers.Select(s => s.sharedMaterial).ToList();
        }

        public async void Show(int damage, Vector3 hitPoint, DamageTextType type = DamageTextType.Default)
        {
            if (_isShowTextDamage)
            {
                hitPoint += new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), Random.Range(-.2f, .2f)) * _reactionForce;
                TextEffect.Create(TextEffectType.Damage,(int)type, hitPoint,  Mathf.Abs(damage).ToString(), _reactionForce);
                _reactionForce = Mathf.Min(_reactionForce + REACTION_FORCE_ADDED, MAX_REACTION_FORCE);
            }
            
            if(_animate)
                return;

            _animate = true;
            ToReactionMaterials();
            await UniTask.Delay(TimeSpan.FromSeconds(DURATION));
            if(_animate)
            {
                ResetMaterials();
                await UniTask.Delay(TimeSpan.FromSeconds(DELAY_AFTER_ANIMATION));
                _animate = false;
            }
        }

        private void Update()
        {
            if (_reactionForce > 1f)
            {
                _reactionForce = Mathf.Max(_reactionForce - Time.deltaTime * REACTION_FORCE_HIDE_MULTIPLIER, 1f);
            }
        }

        private void ToReactionMaterials()
        {
            for(int i = 0; i< _renderers.Count;i++)
            {
                _renderers[i].sharedMaterial = _reactionMaterial;
            }
        }

        private void ResetMaterials()
        {
            
            if(_defaultMaterials.IsNullOrEmpty())
                return;
            
            for(int i = 0; i< _renderers.Count;i++)
            {
                _renderers[i].sharedMaterial = _defaultMaterials[i];
            }
        }

        private void OnDestroy()
        {
            _animate = false;
            ResetMaterials();
        }
        private void OnDisable()
        {
            _animate = false;
            ResetMaterials();
        }
    }
}