using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// An alias of <see cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}"/> that exposes
    /// <see cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}.CollectionChanged"/>.
    /// </summary>
    /// <typeparam name="T">The collection's element type</typeparam>
    /// <remarks>
    /// Required because .NET doesn't currently want to introduce a binary breaking change
    /// https://github.com/dotnet/runtime/issues/14267
    /// </remarks>
    public class ReadOnlyObservableCollection<T> : System.Collections.ObjectModel.ReadOnlyObservableCollection<T>
    {
        /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged"/>
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }

        /// <inheritdoc cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}"/>
        public ReadOnlyObservableCollection(ObservableCollection<T> list) : base(list) { }
    }
}