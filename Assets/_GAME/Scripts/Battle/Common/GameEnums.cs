namespace _GAME.Scripts.Common
{
    public enum Team
    {
        None,
        Player,
        Enemy,
        Ally
    }

    public enum DamageRepeaterType
    {
        Default,
        Head,
        Weak,
        Shield,
        Other
    }

    public enum EffectAttributeType
    {
        None,
        ExplosionDamage,
        HeadShot,
        WeakShot,
        ShieldShot
    }

    public enum WeaponType
    {
        Riffle
    }

    public enum BulletType
    {
        Default
    }
    
    public enum ExplosionType
    {
        Default
    }

    public enum LevelStageType
    {
        Start,
        End,
        Normal,
        Rotate
    }

    public enum PlayerPositionType
    {
        Start,
        End,
        Default,
        
    }

    public enum GameCameraType
    {
        Run,
        Battle,
        Dead,
        Victory
    }

    public enum TextEffectType
    {
        Default,
        Damage,
        Bonus
    }

    public enum DamageTextType
    {
        Default,
        Armor,
        Headshot,
        Weak,
        Explosion
    }

    public enum GameEffectType
    {
        None = -1,
        BlueEnergyExplode,
        LaserHitDecal,
        Explosion_Default,
        Spawn_Default
    }

    public enum EnemyType
    {
        None,
        PIRATE_RIFFLEMAN,
        PIRATE_BOMBER,
        DRONE_BOMBER,
        DRONE_RIFFLE,
        DRONE_LASER,
        PIRATE_BIG_RIFFLEMAN,
    }
    
    public enum EnemyClassType
    {
        None,
        SOLDIER,
        DRONE,
        WEHICLE,
        SHIP,
        BIG_SOLDIER
    }
    
    public enum EnemySubClassType
    {
        Default,
        BOSS
    }
    
    public enum MarkerType
    {
        ENEMY,
        BOSS,
        WARNING
    }
}