using Anvil.CSharp.Core;

namespace Anvil.CSharp.Data
{
    public class DoubleBuffer<T> : AbstractAnvilDisposable
        where T:class, new()
    {
        private readonly T[] m_Buffers = {new T(), new T()};
        private int m_CurrentIndex;

        public T Current => m_Buffers[m_CurrentIndex];
        public T Previous => m_Buffers[1 - m_CurrentIndex];

        public void SwapBuffer()
        {
            m_CurrentIndex = 1 - m_CurrentIndex;
        }
    }
}

