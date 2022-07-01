using System.Collections.Generic;

namespace ScratchGames.StationX.Game
{
    public class CountedSet<T>
    {
        private readonly Dictionary<T, int> m_Items;

        public Dictionary<T, int>.KeyCollection Items
        {
            get => m_Items.Keys;
        }

        public CountedSet()
        {
            m_Items = new Dictionary<T, int>();
        }

        public bool Add(T item)
        {
            if (!m_Items.ContainsKey(item))
            {
                m_Items.Add(item, 1);
                return true;
            }

            m_Items[item]++;
            return false;
        }

        public bool Remove(T item)
        {
            int count = m_Items[item];
            count--;

            if (count == 0)
            {
                m_Items.Remove(item);
                return true;
            }

            m_Items[item] = count;
            return false;
        }

        public void Clear()
        {
            m_Items.Clear();
        }

    }
}
