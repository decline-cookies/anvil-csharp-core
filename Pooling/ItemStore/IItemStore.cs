using System.Collections.Generic;

namespace Anvil.CSharp.Pooling
{
    internal interface IItemStore<T> : IEnumerable<T>
    {
        int Count { get; }

        void Add(T instance);

        T Remove();
    }
}
