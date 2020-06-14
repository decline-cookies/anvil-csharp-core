using System.Collections.Generic;

namespace Anvil.CSharp.Pooling
{
    /// <summary>
    /// Delegate for a function that returns a new <typeparamref name="T"/> instance for populating a pool.
    /// </summary>
    /// <typeparam name="T">The type created.</typeparam>
    public delegate T InstanceCreator<out T>();

    /// <summary>
    /// Delegate for a function that handles disposing <typeparamref name="T"/> instances left in a pool when
    /// the pool is disposed.
    /// </summary>
    /// <param name="instanceList">The list of <typeparamref name="T"/> instances to be disposed.</param>
    /// <typeparam name="T">The type to be disposed.</typeparam>
    public delegate void InstanceDisposer<T>(List<T> instanceList);
}
