using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;

namespace _GAME.Scripts.Battle.Weapons
{
    [Serializable]
    public class DamageDealerData
    {
        public IDamageDealer DamageDealer { get; protected set; } = null;
        public Team Team => DamageDealer.Team;
        
        public virtual void Setup(IDamageDealer damageDealer)
        {
            DamageDealer = damageDealer;
        }
    }
}