using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Pooling
{
    public class Pool<T> : AbstractAnvilDisposable where T : class
    {
        private readonly HashSet<T> m_InstanceSet = new HashSet<T>();

        private readonly InstanceCreator<T> m_InstanceCreator;
        private readonly InstanceDisposer<T> m_InstanceDisposer;

        private readonly IGrowthOperator m_GrowthOperator;

        private readonly int m_InitialCount;
        private readonly int? m_MaxCount;

        private int m_InstanceCount;

        public Pool(InstanceCreator<T> instanceCreator,
            InstanceDisposer<T> instanceDisposer = null,
            int initialCount = 0,
            int? maxCount = null,
            IGrowthOperator growthOperator = null)
        {
            m_InstanceCreator = instanceCreator;
            m_InstanceDisposer = instanceDisposer;

            m_InitialCount = Math.Max(0, initialCount);
            m_MaxCount = maxCount;

            m_GrowthOperator = growthOperator ?? new MultiplicativeGrowth(2);

            if (m_MaxCount.HasValue)
            {
                m_MaxCount = Math.Max(1, m_MaxCount.Value);
                m_InitialCount = Math.Min(m_MaxCount.Value, m_InitialCount);
            }

            Grow(m_InitialCount);
        }

        public void Grow(int step)
        {
            for (int i = 0; i < step; i++)
            {
                CreateInstance();
            }
        }

        public void Populate(IEnumerable<T> instances)
        {
            foreach (T instance in instances)
            {
                AddInstance(instance);
            }
        }

        public T Acquire()
        {
            if (m_InstanceSet.Count == 0)
            {
                Grow(GetGrowthStep());
            }

            T instance = m_InstanceSet.First();
            m_InstanceSet.Remove(instance);
            return instance;
        }

        public void Release(T instance)
        {
            AddInstance(instance);

            // Track the maximum instance count, in case of released instances that were not created by the pool
            m_InstanceCount = Math.Max(m_InstanceCount, m_InstanceSet.Count);
        }

        private void CreateInstance()
        {
            m_InstanceCount++;
            AddInstance(m_InstanceCreator.Invoke());
        }

        private void AddInstance(T instance) {
            Debug.Assert(m_InstanceSet.Add(instance), "Instance already exists in pool!");
        }

        private int GetGrowthStep()
        {
            int growthStep = Math.Max(1, m_GrowthOperator.CalculateGrowthStep(m_InstanceCount));

            if (m_MaxCount.HasValue)
            {
                growthStep = Math.Min(growthStep, m_MaxCount.Value - m_InstanceCount);

                if (growthStep <= 0)
                {
                    throw new Exception(
                        $"Failed to increase pool size, max instances ({m_MaxCount}) already exist!");
                }
            }

            return growthStep;
        }

        protected override void DisposeSelf()
        {
            if (m_InstanceDisposer != null && m_InstanceSet.Any())
            {
                m_InstanceDisposer(m_InstanceSet.ToList());
            }
        }
    }
}
