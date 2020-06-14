using System.Collections.Generic;

namespace Anvil.CSharp.Pooling
{
    public delegate T InstanceCreator<out T>();

    public delegate void InstanceDisposer<T>(List<T> instanceList);
}
