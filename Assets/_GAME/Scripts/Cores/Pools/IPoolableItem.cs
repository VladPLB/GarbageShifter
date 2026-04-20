namespace _GAME.Scripts.Pools
{
    public interface IPoolableItem
    {
    }
    public interface IPoolableItem<T>: IPoolableItem
    {
        T Type { get; }
    }
}