using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts
{
    public class PoolProvider: MonoBehaviour, IRuntimeSetup
    {
        public Pool<Bullet,BulletType> Bullets { get; private set; }
        public Pool<Explosion,ExplosionType> Explosions { get; private set; }
        public Pool<TextEffect,TextEffectType> TextEffects { get; private set; }
        public Pool<GameEffect,GameEffectType> GameEffects { get; private set; }
        
        public Pool<UIMarker,MarkerType> UIMarkers { get; private set; }
        public Pool<UIEnemyHealthBar,EnemySubClassType> UIEnemyHealthBar { get; private set; }
        
        public Pool<EnemyController,EnemyType> Enemies { get; private set; }
        private void Awake()
        {
            Core.Registry(this, typeof(DataBase));
        }

        public void RuntimeSetup()
        {
            var database = Core.Get<DataBase>();
            Bullets = new Pool<Bullet, BulletType>(database.Bullets.GetPrefab);
            Explosions = new Pool<Explosion, ExplosionType>(database.Explosion.GetPrefab);
            TextEffects = new Pool<TextEffect, TextEffectType>(database.TextsEffects.GetPrefab);
            GameEffects = new Pool<GameEffect,GameEffectType>(database.GameEffects.GetPrefab);
            Enemies = new Pool<EnemyController, EnemyType>(database.Enemies.GetPrefab);
            UIMarkers = new Pool<UIMarker,MarkerType>(database.UIEnemyMarkers.GetPrefab);
            UIEnemyHealthBar = new Pool<UIEnemyHealthBar,EnemySubClassType>(database.UIEnemyHealthBars.GetPrefab);
        }
    }
}