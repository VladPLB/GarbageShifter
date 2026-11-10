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
        RiffleLaser = 0,
        MinigunLaser = 1,
        PistolLaser = 10,
        RevolverLaser = 20,
        ShotgunLaser = 30,

        // Ranked variants: base value + 100 * ItemRank
        RiffleLaser_Uncommon = 100,
        MinigunLaser_Uncommon = 101,
        PistolLaser_Uncommon = 110,
        RevolverLaser_Uncommon = 120,
        ShotgunLaser_Uncommon = 130,

        RiffleLaser_Rare = 200,
        MinigunLaser_Rare = 201,
        PistolLaser_Rare = 210,
        RevolverLaser_Rare = 220,
        ShotgunLaser_Rare = 230,

        RiffleLaser_Epic = 300,
        MinigunLaser_Epic = 301,
        PistolLaser_Epic = 310,
        RevolverLaser_Epic = 320,
        ShotgunLaser_Epic = 330,

        RiffleLaser_Legendary = 400,
        MinigunLaser_Legendary = 401,
        PistolLaser_Legendary = 410,
        RevolverLaser_Legendary = 420,
        ShotgunLaser_Legendary = 430,

        RiffleLaser_Unique = 500,
        MinigunLaser_Unique = 501,
        PistolLaser_Unique = 510,
        RevolverLaser_Unique = 520,
        ShotgunLaser_Unique = 530,
    }

    public enum ArmorType
    {
        // Focus: Health / Defense / EnergyShield. Ranked variants: base value + 100 * ItemRank
        ArmorHealth = 0,
        ArmorDefense = 1,
        ArmorShield = 2,

        ArmorHealth_Uncommon = 100,
        ArmorDefense_Uncommon = 101,
        ArmorShield_Uncommon = 102,

        ArmorHealth_Rare = 200,
        ArmorDefense_Rare = 201,
        ArmorShield_Rare = 202,

        ArmorHealth_Epic = 300,
        ArmorDefense_Epic = 301,
        ArmorShield_Epic = 302,

        ArmorHealth_Legendary = 400,
        ArmorDefense_Legendary = 401,
        ArmorShield_Legendary = 402,

        ArmorHealth_Unique = 500,
        ArmorDefense_Unique = 501,
        ArmorShield_Unique = 502,
    }

    public enum ProjectileDamageMode
    {
        Payload,        // damage goes to the bullet payload (explosion)
        Hit,            // damage on direct hit only
        HitAndPayload,
    }

    public enum SecondaryWeaponType
    {
        // Ranked variants: base value + 100 * ItemRank
        RocketLauncher = 0,
        RocketLauncher_Uncommon = 100,
        RocketLauncher_Rare = 200,
        RocketLauncher_Epic = 300,
        RocketLauncher_Legendary = 400,
        RocketLauncher_Unique = 500,
    }

    public enum BulletType
    {
        Default,
        Default_blue,
        Laser_red,
        Laser_blue,
        Rocket_red
    }
    
    public enum CoinType
    {
        Default,
        Metal,
        Blueprint,
        Scrap,
        Material,
        Electronic
    }

    public enum RareType
    {
        Common,
        Uncommon,
        Unique,
        Special,
        Rare,
        Legendary,
        Universe,
        BlackHole
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
        Rotate,
        Rage,
        Boss,
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
        Victory,
        BigWeapon
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