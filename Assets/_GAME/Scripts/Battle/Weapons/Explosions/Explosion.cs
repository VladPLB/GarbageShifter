using System;
using _GAME.Scripts;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Pools;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Explosion : MonoBehaviour, IPoolableItem<ExplosionType>
{
   [SerializeField]
   private ExplosionType _type = ExplosionType.Default;
   
   [SerializeField] private LayerMask _coliderMask;
   [SerializeField] private GameEffectType _effectType = GameEffectType.Explosion_Default;
   [SerializeField] private ExplosionData _defaultData;
   
   private ExplosionData _data;
   
   public ExplosionType Type => _type;
   public bool IsActive => _data != null;
   
   public void Setup()
   {
      Setup(Core.Get<LevelController>().Player);
   }

   // damage < 0 = default explosion damage
   public void Setup(IDamageDealer damageDealer, int damage = -1)
   {
      _data = _defaultData;
      _data.Setup(damageDealer, damage, new ExplosionEffectAttribute());
      Explode();
   }

   public void Setup(ExplosionData data)
   {
      _data = new ExplosionData();
      _data.Setup(data);
      Explode();
   }

   private void Explode()
   {
      if(!IsActive)
         return;

      // Explosions hit everyone, but keep the player's team so the player is not hurt by own explosions
      var team = _data.DamageDealer != null && _data.DamageDealer.Team == Team.Player ? Team.Player : Team.None;
      var colliders = Physics.OverlapSphere(transform.position, _data.Radius, _coliderMask);
      for (int i = 0; i < colliders.Length; i++)
      {
         var damageReceiver = colliders[i].GetComponent<IDamageReceiver>();
         if(damageReceiver!=null)
         {
            damageReceiver.OnDamage(team, _data.Damage, transform.position, _data.Attributes);
         }
      }

      var gameEffect = GameEffect.Create(_effectType, transform.position);
      EventBus.Push(new SoundPlayEvent(SoundType.Explosion, transform.position), EventBus.EventRegion.GLOBAL);

      Remove();
   }

   public void Remove()
   {
      _data = null;
      Core.Get<PoolProvider>().Explosions.Push(this);
   }

   public static Explosion Create(ExplosionType type)
   {
      return Core.Get<PoolProvider>().Explosions.Pop(type);
   }

   private void OnDrawGizmosSelected()
   {
      if (_defaultData != null)
      {
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(transform.position, _defaultData.Radius);
      }
   }
}
