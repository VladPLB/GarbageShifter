using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Common
{
    public interface IDamageReceiver:IBattleItem
    {
        void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null);
    }
}