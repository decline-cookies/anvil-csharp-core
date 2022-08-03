namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// A node in a <see cref="LinkedQueue{T}"/>. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">The item type to store.</typeparam>
    public sealed class LinkedQueueNode<T>
    {
        /// <summary>
        /// The Data to store in the List
        /// </summary>
        public readonly T Data;

        /// <summary>
        /// The next node attached to this one. Could be null.
        /// </summary>
        public LinkedQueueNode<T> NextNode;

        /// <summary>
        /// The previous node attached to this one. Could be null.
        /// </summary>
        public LinkedQueueNode<T> PrevNode;

        public LinkedQueueNode(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Unlinks this node from a <see cref="LinkedQueue{T}"/> in a non-destructive way by ensuring the list is
        /// stitched together correctly.
        /// </summary>
        public void Unlink()
        {
            if (NextNode != null)
            {
                NextNode.PrevNode = PrevNode;
            }

            if (PrevNode != null)
            {
                PrevNode.NextNode = NextNode;
            }

            NextNode = PrevNode = null;
        }
    }
}