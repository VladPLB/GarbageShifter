using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Weapons.Bullets
{
    [Serializable]
    public class BulletData: DamageDealerData
    {
        [SerializeField] private BulletType _type;
        [SerializeField] private float _speed;
        
        public int Damage{ get; private set; }
        // Damage passed to payload handlers (explosion etc.), -1 = payload uses its own default
        public int PayloadDamage{ get; private set; } = -1;
        public Vector3 MoveDirection{ get; private set; }
        public List<IEffectAttribute> Attributes { get; private set; }
        
        public BulletType Type => _type;
        public float Speed => _speed;
        public Vector3 Velocity => MoveDirection * _speed;

        public void Setup(IDamageDealer damageDealer, int damage, Vector3 moveDirection, params IEffectAttribute[] attributes)
        {
            base.Setup(damageDealer);
            Damage = damage;
            PayloadDamage = -1;
            MoveDirection = moveDirection;
            Attributes = attributes.IsNullOrEmpty()? new(): attributes.ToList();
        }

        public void SetPayloadDamage(int damage)
        {
            PayloadDamage = damage;
        }
    }
}