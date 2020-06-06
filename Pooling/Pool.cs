using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Pooling
{
    public class Pool<T> : AbstractAnvilDisposable
    {
        private readonly PoolSettings m_Settings;
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly InstanceCreator<T> m_InstanceCreator;

        private int m_InstanceCount;

        public Pool(InstanceCreator<T> instanceCreator, PoolSettings settings = null)
        {
            m_InstanceCreator = instanceCreator;
            m_Settings = settings ?? PoolSettings.DEFAULT;

            for (int i = 0; i < m_Settings.InitialCount; i++)
            {
                CreateInstance();
            }
        }

        public T Acquire()
        {
            if (m_Stack.Count == 0)
            {
                Grow();
            }

            return m_Stack.Pop();
        }

        public void Release(T instance)
        {
            m_Stack.Push(instance);

            // Track the maximum instance count, in case of released instances that were not created by the pool
            m_InstanceCount = Math.Max(m_InstanceCount, m_Stack.Count);
        }

        private void Grow()
        {
            int growthStep = Math.Max(1, m_Settings.GrowthOperator.CalculateGrowthStep(m_InstanceCount));

            if (m_Settings.MaxCount.HasValue)
            {
                growthStep = Math.Min(growthStep, m_Settings.MaxCount.Value - m_InstanceCount);

                if (growthStep <= 0)
                {
                    throw new Exception(
                        $"Failed to increase pool size, max instances ({m_Settings.MaxCount}) already exist!");
                }
            }

            for (int i = 0; i < growthStep; i++)
            {
                CreateInstance();
            }
        }

        private void CreateInstance()
        {
            m_InstanceCount++;
            m_Stack.Push(m_InstanceCreator.Invoke());
        }

        protected override void DisposeSelf()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                foreach (T instance in m_Stack)
                {
                    ((IDisposable)instance).Dispose();
                }
            }
        }
    }
}
