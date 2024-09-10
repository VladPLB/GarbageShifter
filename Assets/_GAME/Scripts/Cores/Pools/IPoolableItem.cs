namespace _GAME.Scripts.Pools
{
    public interface IPoolableItem<T>
    {
        T Type { get; }
    }
}