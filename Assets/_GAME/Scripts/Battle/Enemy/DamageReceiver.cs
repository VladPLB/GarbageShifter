using System.Collections.Generic;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public abstract class DamageReceiver : MonoBehaviour, IDamageReceiver
    {
        public abstract Team Team { get;}

        public abstract void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null);
    }
}