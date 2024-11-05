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
        [SerializeField] private int _damage;
        [SerializeField] private float _radius;
        public int Damage{ get; private set; }
        public List<IEffectAttribute> Attributes { get; private set; }

        public float Radius => _radius;

        public void Setup(IDamageDealer damageDealer, int damage = -1, params IEffectAttribute[] attributes)
        {
            base.Setup(damageDealer);
            Damage = damage >= 0 ? damage : _damage;
            Attributes = attributes.IsNullOrEmpty()? new(): attributes.ToList();
        }
    }
}