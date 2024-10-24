using System;
using _GAME.Scripts;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Common;
using _GAME.Scripts.Pools;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolableItem<BulletType>
{
   protected const float SPEED_BASE_MULTIPLIER = 1f;
   protected const float REMOVE_DELAY = 1f;
   protected const float MAX_LIVE_TIME = 5f;
   
   [SerializeField]
   private BulletType _type = BulletType.Default;

   [SerializeField] private float _coliderRadius = .1f;
   [SerializeField] private LayerMask _coliderMask;
   [SerializeField] private GameEffectType _hitImpactType = GameEffectType.LaserHitDecal;
   
   [SerializeField]
   private BulletViewer _viewer;
   
   private BulletData _data;
   private float _maxLiveTime;
   
   private Action _onHit;


   public BulletType Type => _type;
   public bool IsActive => _data != null;

   public void Setup(BulletData data, Action onHit)
   {
      _data = data;
      _onHit = onHit;
      _type = _data.Type;
      _viewer.Setup();
      _maxLiveTime = Time.time + MAX_LIVE_TIME;
   }

   private void FixedUpdate()
   {
      if(!IsActive)
         return;
      var ray = new Ray(transform.position, transform.forward);
      transform.position += _data.Velocity * (SPEED_BASE_MULTIPLIER * Time.fixedDeltaTime);
      if (Physics.SphereCast(ray, _coliderRadius, out var hit,
             _data.Speed * (SPEED_BASE_MULTIPLIER * Time.fixedDeltaTime), _coliderMask))
      {
         OnHit(hit);
      }
      if(Time.time>_maxLiveTime)
      {
         Remove();
      }
   }

   private void OnHit(RaycastHit hit)
   {
      Vector3 hitPoint = hit.point;
      var damageReceived = hit.transform.GetComponent<IDamageReceiver>();
      if (damageReceived != null)
      {
         damageReceived.OnDamage(_data.DamageDealer.Team, _data.Damage, hitPoint, _data.Attributes);
         _onHit?.Invoke();
         _onHit = null;
      }
      
      var isSpawnDecal = damageReceived == null;
      Vector3 hitNormal = hit.normal;
      _viewer.Hit(hitNormal);
      if (isSpawnDecal)
      {
         SpawnDecal(hit.transform, hitPoint, hitNormal);
      }

      Remove();
   }

   private void SpawnDecal(Transform collision, Vector3 hitPoint, Vector3 hitNormal)
   {
      var decal = GameEffect.Create(_hitImpactType, hitPoint);
      decal.transform.forward = hitNormal;
      decal.transform.parent = collision;
   }

   public async void Remove()
   {
      _data = null;

      await UniTask.Delay(TimeSpan.FromSeconds(REMOVE_DELAY));
      
      Core.Get<PoolProvider>().Bullets.Push(this);
   }

   public static Bullet Create(BulletType type)
   {
      return Core.Get<PoolProvider>().Bullets.Pop(type);
   }
}
