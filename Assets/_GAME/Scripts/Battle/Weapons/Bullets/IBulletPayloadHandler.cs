namespace _GAME.Scripts.Weapons.Bullets
{
    // Bridge for bullet dead handlers that need the bullet's damage data (e.g. explosion of a rocket)
    public interface IBulletPayloadHandler
    {
        void OnDead(BulletData data);
    }
}
