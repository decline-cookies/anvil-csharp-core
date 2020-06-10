using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Pooling
{
    public class Pool<T> : AbstractAnvilDisposable where T : class
    {
        public event InstanceDisposer<T> InstanceDisposer;

        private readonly PoolSettings m_Settings;
        private readonly HashSet<T> m_Set = new HashSet<T>();
        private readonly InstanceCreator<T> m_InstanceCreator;

        private int m_InstanceCount;

        public Pool(InstanceCreator<T> instanceCreator, PoolSettings settings = null)
        {
            m_InstanceCreator = instanceCreator;
            m_Settings = settings ?? PoolSettings.DEFAULT;

            Grow(m_Settings.InitialCount);
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
            if (m_Set.Count == 0)
            {
                Grow(GetGrowthStep());
            }

            T instance = m_Set.First();
            m_Set.Remove(instance);
            return instance;
        }

        public void Release(T instance)
        {
            AddInstance(instance);

            // Track the maximum instance count, in case of released instances that were not created by the pool
            m_InstanceCount = Math.Max(m_InstanceCount, m_Set.Count);
        }

        private void CreateInstance()
        {
            m_InstanceCount++;
            AddInstance(m_InstanceCreator.Invoke());
        }

        private void AddInstance(T instance) => Debug.Assert(m_Set.Add(instance), "Instance already exists in pool!");

        private int GetGrowthStep()
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

            return growthStep;
        }

        private void Grow(int step)
        {
            for (int i = 0; i < step; i++)
            {
                CreateInstance();
            }
        }

        protected override void DisposeSelf()
        {
            foreach (T instance in m_Set)
            {
                InstanceDisposer?.Invoke(instance);
            }
        }
    }
}
