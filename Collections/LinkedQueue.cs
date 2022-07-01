namespace ScratchGames.StationX.Game
{
    public class LinkedQueue<T>
    {
        private readonly LinkedQueueLink<T> m_First = new LinkedQueueLink<T>(default);
        private readonly LinkedQueueLink<T> m_Last = new LinkedQueueLink<T>(default);

        /// <summary>
        /// Is this LinkedQueue Empty?
        /// </summary>
        public bool IsEmpty
        {
            get => m_First.NextLink == m_Last;
        }

        public LinkedQueue()
        {
            Reset();
        }

        /// <summary>
        /// Queues a <see cref="LinkedQueueLink{T}"/> in the list.
        /// </summary>
        /// <param name="link">The <see cref="LinkedQueueLink{T}"/> to append.</param>
        public void Enqueue(LinkedQueueLink<T> link)
        {
            LinkedQueueLink<T> prevLink = m_Last.PrevLink;

            prevLink.NextLink = link;
            link.PrevLink = prevLink;

            m_Last.PrevLink = link;
            link.NextLink = m_Last;
        }

        /// <summary>
        /// Dequeues a <see cref="LinkedQueueLink{T}"/> from the list.
        /// </summary>
        /// <returns>The <see cref="LinkedQueueLink{T}"/> that was removed from the list.</returns>
        public T Dequeue()
        {
            LinkedQueueLink<T> link = m_First.NextLink;
            T data = link.Data;

            //Stitch the list together by taking this link out
            m_First.NextLink = link.NextLink;

            link.NextLink = null;
            link.PrevLink = null;

            return data;
        }

        /// <summary>
        /// Resets the list in a destructive manner. Existing links may still be linked to each other but this list has
        /// no knowledge of them anymore and considers itself empty.
        /// </summary>
        public void Reset()
        {
            m_First.NextLink = m_Last;
            m_Last.PrevLink = m_First;
        }
    }
}

