using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Pooling
{
    public class Pool<T> : AbstractAnvilDisposable
    {
        private readonly PoolSettings m_Settings;
        private readonly IItemStore<T> m_ItemStore;
        private readonly Func<T> m_CreateInstanceFunc;

        private int m_InstanceCount;

        public Pool(Func<T> createInstanceFunc, PoolSettings settings = null)
        {
            m_CreateInstanceFunc = createInstanceFunc;
            m_Settings = settings ?? new PoolSettings();

            switch(m_Settings.ItemStoreType)
            {
                case ItemStoreType.Queue:
                    m_ItemStore = new QueueItemStore<T>();
                    break;
                case ItemStoreType.Stack:
                    m_ItemStore = new StackItemStore<T>();
                    break;
                default:
                    throw new NotImplementedException($"{m_Settings.ItemStoreType} item store is not implemented");
            }

            for (int i = 0; i < m_Settings.InitialCount; i++)
            {
                CreateInstance();
            }
        }

        public T Acquire()
        {
            if (m_ItemStore.Count == 0)
            {
                int growthStep = Math.Max(1, m_Settings.GrowthOperator.CalculateGrowthStep(m_InstanceCount));

                if (m_Settings.MaxCount.HasValue)
                {
                    growthStep = Math.Min(growthStep, m_Settings.MaxCount.Value - m_InstanceCount);

                    if (growthStep <= 0)
                    {
                        return default;
                    }
                }

                for (int i = 0; i < growthStep; i++)
                {
                    CreateInstance();
                }
            }

            T instance = m_ItemStore.Remove();

            if (instance is IPoolable poolable)
            {
                poolable.OnAcquired();
            }

            return instance;
        }

        public void Release(T instance)
        {
            // Track the maximum instance count, in case of released instances that were not created by the pool
            m_InstanceCount = Math.Max(m_InstanceCount, m_ItemStore.Count);

            if (instance is IPoolable poolable)
            {
                poolable.OnReleased();
            }

            m_ItemStore.Add(instance);
        }

        private void CreateInstance()
        {
            m_InstanceCount++;
            m_ItemStore.Add(m_CreateInstanceFunc());
        }

        protected override void DisposeSelf()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                foreach (T instance in m_ItemStore)
                {
                    ((IDisposable)instance).Dispose();
                }
            }
        }
    }
}
