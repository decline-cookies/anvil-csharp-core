using System.Collections.Generic;

namespace Anvil.CSharp.Pooling
{
    internal class StackItemStore<T> : Stack<T>, IItemStore<T>
    {
        public void Add(T instance) => Push(instance);

        public T Remove() => Pop();
    }
}
