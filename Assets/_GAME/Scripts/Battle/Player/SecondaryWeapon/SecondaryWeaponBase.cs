using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Player.SecondaryWeapon
{
    public abstract class SecondaryWeaponBase : MonoBehaviour
    {
        protected static readonly TimeSpan DelayAfterShowing = TimeSpan.FromSeconds(0f); 
        
        [SerializeField] protected SecondaryWeaponDisolver _disolver;

        protected Player _player;
        protected bool _isReady = false;
        protected bool _isFire = false;

        public virtual bool IsReady => _isReady;
        public virtual bool IsFire => _isFire;

        public virtual void Setup(Player player)
        {
            _player = player;
            SetReady(false);
            _disolver.Setup();
        }
        
        public virtual void Show()
        {
            _disolver.Show(()=> SetReady(true));
        }

        public virtual async void SetReady(bool ready)
        {
            if (ready && DelayAfterShowing.Milliseconds>0)
                await UniTask.Delay(DelayAfterShowing);
            _isReady = ready;
        }

        public virtual void OnFire()
        {
            _isFire = true;
            Hide();
        }

        public virtual void Hide()
        {
            SetReady(false);
            _disolver.Hide(() => _isFire = false);
        }
    }
}