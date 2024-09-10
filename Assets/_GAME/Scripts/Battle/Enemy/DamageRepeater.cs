using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class DamageRepeater: DamageReceiver
    {
        [SerializeField] private DamageRepeaterType _damageRepeaterType = DamageRepeaterType.Default;
        [SerializeField] private List<Collider> _colliders = new();

        private IDamageReceiver _damagedReceiver;

        public override Team Team => _damagedReceiver?.Team ?? Team.None;

        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            if(damageDealersTeam == Team)
                return;
            
            if(additiveAttributes!=null && additiveAttributes.Any(a=>a.Key == EffectAttributeType.SplashDamage) && _damageRepeaterType!= DamageRepeaterType.Default)
                return;
            
            var shotAttribute = _damageRepeaterType switch
            {
                DamageRepeaterType.Head => new HeadShotEffectAttribute(),
                DamageRepeaterType.Weak => new HeadShotEffectAttribute(),
                DamageRepeaterType.Shield => new HeadShotEffectAttribute(),
                _ => null
            };
            
            if (shotAttribute != null)
            {
                additiveAttributes ??= new List<IEffectAttribute>();
                if (!additiveAttributes.Any(a => a.Key == shotAttribute.Key))
                {
                    additiveAttributes.Add(shotAttribute);
                }
            }
            _damagedReceiver?.OnDamage(damageDealersTeam, damageAmount, hitPoint, additiveAttributes);
        }

        public void SetActive(bool isActive)
        {
            if (_damagedReceiver == null || _colliders.IsNullOrEmpty())
                isActive = false;

            enabled = isActive;
            _colliders.ForEach(c=>c.enabled = enabled);
        }
        
        public void ApplyReceiver(IDamageReceiver damageReceiver)
        {
            _damagedReceiver = damageReceiver;
        }
        
        public void FindColliders()
        {
            _colliders = GetComponents<Collider>().ToList();
        }
    }
}