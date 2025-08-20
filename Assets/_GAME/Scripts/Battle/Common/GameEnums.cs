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
        Default,
        Default_blue,
        Laser_red,
        Laser_blue,
        Rocket_red
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
    
    public enum LobbyPlaceType
    {
        Map = -1,
        Transition = 0,
        Bar_Barmen = 10,
        Bar_BlackMarket = 11,
        Angar_Squad = 20,
        Angar_Ship = 21,
        Angar_Master = 22,
        Lab_Researcher = 30,
        Lab_Medic = 31,
        Lab_Medic_Tut_NewLife = 32,
        Shop_Market = 40,
        Shop_Teleport = 41
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
        Spawn_Default,
        Explosion_Small
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
        BOT_RIFFLE,
        BOT_DESANT,
        BOT_DRONE
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
    
    public enum AllyType
    {
        TUTORIAL
    }
    
    public enum AllyClassType
    {
        None,
        SOLDIER,
        DRONE,
    }
    
    
    public enum MarkerType
    {
        ENEMY,
        BOSS,
        WARNING
    }
}