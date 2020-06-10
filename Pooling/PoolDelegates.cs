namespace Anvil.CSharp.Pooling
{
    public delegate T InstanceCreator<out T>();

    public delegate void InstanceDisposer<in T>(T instance);
}
