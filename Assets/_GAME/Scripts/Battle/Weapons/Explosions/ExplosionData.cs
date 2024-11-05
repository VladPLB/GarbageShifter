using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Weapons.Bullets
{
    [Serializable]
    public class ExplosionData: DamageDealerData
    {
        [SerializeField] private ExplosionType _type;
        [SerializeField] private int _damage;
        [SerializeField] private float _radius;
        
        
        public int Damage{ get; private set; }
        public List<IEffectAttribute> Attributes { get; private set; } = new();

        public ExplosionType Type=> _type;
        public float Radius => _radius;

        public void Setup(ExplosionData data, params IEffectAttribute[] attributes)
        {
            _type = data._type;
            Damage = _damage = data._damage;
            _radius = data._radius;
            Attributes = attributes.IsNullOrEmpty()? data.Attributes: attributes.ToList();
            base.Setup(data.DamageDealer);
        }

        public void Setup(IDamageDealer damageDealer, int damage = -1, params IEffectAttribute[] attributes)
        {
            base.Setup(damageDealer);
            Damage = damage >= 0 ? damage : _damage;
            Attributes = attributes.IsNullOrEmpty()? new(): attributes.ToList();
        }
    }
}