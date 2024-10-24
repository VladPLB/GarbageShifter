using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.Weapons.Bullets
{
    public class BulletViewer: MonoBehaviour
    {
        [SerializeField] private List<GameObject> _activatedItemsAfterHit = new();
        [SerializeField] private List<GameObject> _deactivatedItemsAfterHit = new();
        [SerializeField] private List<TrailRenderer> _trails = new();
        
        public void Setup()
        {
            _deactivatedItemsAfterHit.ForEach(a=> a.SetActive(true));
            _activatedItemsAfterHit.ForEach(a=> a.SetActive(false));
            _trails.ForEach(a=> a.Clear());
        }

        public void Hit(Vector3 normal)
        {
            _deactivatedItemsAfterHit.ForEach(a=> a.SetActive(false));
            _activatedItemsAfterHit.ForEach(a=> a.SetActive(true));
        }
    }
}