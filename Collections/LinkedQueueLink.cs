namespace ScratchGames.StationX.Game
{
    public class LinkedQueueLink<T>
    {
        /// <summary>
        /// The Data to store in the List
        /// </summary>
        public readonly T Data;

        /// <summary>
        /// The next link attached to this one. Could be null.
        /// </summary>
        public LinkedQueueLink<T> NextLink;

        /// <summary>
        /// The previous link attached to this one. Could be null.
        /// </summary>
        public LinkedQueueLink<T> PrevLink;

        public LinkedQueueLink(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Unlinks this Link from a <see cref="LinkedQueue{T}"/> in a non-destructive way by ensuring the list is
        /// stitched together correctly.
        /// </summary>
        public void Unlink()
        {
            if (NextLink != null)
            {
                NextLink.PrevLink = PrevLink;
            }

            if (PrevLink != null)
            {
                PrevLink.NextLink = NextLink;
            }

            NextLink = PrevLink = null;
        }
    }
}

