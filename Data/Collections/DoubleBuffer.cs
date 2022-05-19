namespace Anvil.CSharp.Data
{
    /// <summary>
    /// A data structure to hold two of the same objects for easy comparision over time.
    /// </summary>
    /// <typeparam name="T">The type of object to compare.</typeparam>
    public class DoubleBuffer<T>
        where T:new()
    {
        private readonly T[] m_Buffers = {new T(), new T()};
        private int m_CurrentIndex;

        /// <summary>
        /// The Current buffer to use.
        /// </summary>
        public T Current => m_Buffers[m_CurrentIndex];

        /// <summary>
        /// The Previous buffer that was used.
        /// </summary>
        public T Previous => m_Buffers[1 - m_CurrentIndex];

        /// <summary>
        /// Swaps the <see cref="Current"/> and <see cref="Previous"/> buffer.
        /// </summary>
        public void SwapBuffer()
        {
            m_CurrentIndex = 1 - m_CurrentIndex;
        }
    }
}

