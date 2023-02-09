namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// A queue implementation that is backed by a linked list.
    /// Behaves the same as <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items to store.</typeparam>
    public class LinkedQueue<T>
    {
        private readonly LinkedQueueNode<T> m_First = new LinkedQueueNode<T>(default);
        private readonly LinkedQueueNode<T> m_Last = new LinkedQueueNode<T>(default);

        /// <summary>
        /// Is true if the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get => m_First.NextNode == m_Last;
        }

        /// <summary>
        /// Create a new <see cref="LinkedQueue{T}"/> instance.
        /// </summary>
        public LinkedQueue()
        {
            Reset();
        }

        /// <summary>
        /// Queues a <see cref="LinkedQueueNode{T}"/> in the list.
        /// </summary>
        /// <param name="node">The <see cref="LinkedQueueNode{T}"/> to append.</param>
        public void Enqueue(LinkedQueueNode<T> node)
        {
            LinkedQueueNode<T> prevNode = m_Last.PrevNode;

            prevNode.NextNode = node;
            node.PrevNode = prevNode;

            m_Last.PrevNode = node;
            node.NextNode = m_Last;
        }

        /// <summary>
        /// Dequeues a <see cref="LinkedQueueNode{T}"/> from the list.
        /// </summary>
        /// <returns>The <see cref="LinkedQueueNode{T}"/> that was removed from the list.</returns>
        public T Dequeue()
        {
            LinkedQueueNode<T> node = m_First.NextNode;
            T data = node.Data;

            //Stitch the list together by taking this link out
            m_First.NextNode = node.NextNode;

            node.NextNode = null;
            node.PrevNode = null;

            return data;
        }

        /// <summary>
        /// Resets the list in a destructive manner. Existing links may still be linked to each other but this list has
        /// no knowledge of them anymore and considers itself empty.
        /// </summary>
        public void Reset()
        {
            m_First.NextNode = m_Last;
            m_Last.PrevNode = m_First;
        }
    }
}
