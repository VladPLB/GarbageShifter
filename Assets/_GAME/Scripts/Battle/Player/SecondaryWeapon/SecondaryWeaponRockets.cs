using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    public class SecondaryWeaponRockets:SecondaryWeaponBase
    {
        private static readonly TimeSpan _shotDelay = TimeSpan.FromSeconds(.3f);
        private static readonly TimeSpan _shotDelayToHide = TimeSpan.FromSeconds(.3f);
        
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private List<GameObject> _rockets;
        [SerializeField] private List<ParticleSystem> _muzzleFlashes;
        
        private BulletData _defaultBulletData;

        public override void Setup(Player player)
        {
            base.Setup(player);
            _defaultBulletData = Core.Get<DataBase>().Bullets.GetDefaultData(_bulletType);
        }

        public override void Show()
        {
            base.Show();
            foreach (var rocket in _rockets)
            {
                rocket.SetActive(true);
            }
        }

        public override async void OnFire()
        {
            _isFire = true;
            
            for(int i = 0; i< _rockets.Count;i++)
            {
                SpawnProjectile(_rockets[i].transform);
                _muzzleFlashes[i]?.Play();
                _rockets[i].SetActive(false);
                if(i<_rockets.Count-1)
                {
                    await UniTask.Delay(_shotDelay);
                }
            }
            await UniTask.Delay(_shotDelayToHide);
            Hide();
        }

        private void SpawnProjectile(Transform point)
        {
           
            var bullet = Bullet.Create(_bulletType);
            bullet.transform.position = point.position;
            bullet.transform.forward = point.forward;
            var data = _defaultBulletData.Clone();
            data.Setup(_player, 0, bullet.transform.forward);
            bullet.Setup(data, null);
            bullet.gameObject.SetActive(true);
        }
    }
}