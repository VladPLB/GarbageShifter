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
        Shield
    }

    public enum EffectAttributeType
    {
        None,
        SplashDamage,
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
        Weak
    }

    public enum GameEffectType
    {
        BlueEnergyExplode,
        LaserHitDecal,
    }

    public enum EnemyType
    {
        None,
        RIFFLEMAN
    }
    
    public enum EnemyClassType
    {
        None,
        SOLDIER,
        DRONE,
        WEHICLE,
        SHIP
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