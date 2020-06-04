namespace Anvil.CSharp.Pooling
{
    public interface IPoolable
    {
        void OnAcquired();

        void OnReleased();
    }
}
