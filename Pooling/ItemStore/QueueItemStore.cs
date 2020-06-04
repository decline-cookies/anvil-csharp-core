using System.Collections.Generic;

namespace Anvil.CSharp.Pooling
{
    internal class QueueItemStore<T> : Queue<T>, IItemStore<T>
    {
        public void Add(T instance) => Enqueue(instance);

        public T Remove() => Dequeue();
    }
}
