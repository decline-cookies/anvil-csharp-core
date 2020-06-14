﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Pooling
{
    /// <summary>
    /// A managed collection of <typeparamref name="T"/> objects, which may be acquired for use by the application,
    /// then released when no longer needed. This avoids having to create and destroy those instances each time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> : AbstractAnvilDisposable where T : class
    {
        private readonly HashSet<T> m_InstanceSet = new HashSet<T>();

        private readonly InstanceCreator<T> m_InstanceCreator;
        private readonly InstanceDisposer<T> m_InstanceDisposer;

        private readonly IGrowthOperator m_GrowthOperator;

        private readonly int? m_MaxCount;

        private int m_InstanceCount;

        /// <summary>
        /// Constructs a <see cref="Pool{T}"/> with an <see cref="InstanceCreator{T}"/> and optional settings.
        /// </summary>
        /// <param name="instanceCreator">The delegate used to create instances when populating the pool.</param>
        /// <param name="instanceDisposer">The delegate to invoke for disposing any instances in the pool
        /// when <see cref="DisposeSelf"/> is called. If no instances remain in the pool, this is not invoked.</param>
        /// <param name="initialCount">The number of instances the pool should create immediately, to warm up.</param>
        /// <param name="maxCount">The maximum number of instances the pool may create. If <see cref="Acquire"/>
        /// is called while the pool is empty and the maximum has already been reached, an exception is thrown.</param>
        /// <param name="growthOperator">The operator used when the pool increases in size; ex.
        /// <see cref="AdditiveGrowth"/> or <see cref="MultiplicativeGrowth"/>.</param>
        public Pool(InstanceCreator<T> instanceCreator,
            InstanceDisposer<T> instanceDisposer = null,
            int initialCount = 0,
            int? maxCount = null,
            IGrowthOperator growthOperator = null)
        {
            m_InstanceCreator = instanceCreator;
            m_InstanceDisposer = instanceDisposer;

            initialCount = Math.Max(0, initialCount);
            m_MaxCount = maxCount;

            m_GrowthOperator = growthOperator ?? new MultiplicativeGrowth(2);

            if (m_MaxCount.HasValue)
            {
                m_MaxCount = Math.Max(1, m_MaxCount.Value);
                initialCount = Math.Min(m_MaxCount.Value, initialCount);
            }

            Grow(initialCount);
        }

        /// <summary>
        /// Creates <paramref name="step"/> new instances and adds them to the pool.
        /// </summary>
        /// <param name="step">The number of new instances to create.</param>
        public void Grow(int step)
        {
            for (int i = 0; i < step; i++)
            {
                AddInstance(CreateInstance());
            }
        }

        /// <summary>
        /// Inserts externally created <see cref="T"/> instances directly into the pool.
        /// </summary>
        /// <param name="instances">The collection of instances to insert directly into the pool.</param>
        public void Populate(IEnumerable<T> instances)
        {
            foreach (T instance in instances)
            {
                AddInstance(instance);
            }
        }

        /// <summary>
        /// Frees an instance from the pool for use by the application. If the pool is empty, it automatically grows
        /// and returns one of the new instances.
        /// </summary>
        /// <returns>An instance of <see cref="T"/>.</returns>
        /// <exception cref="Exception">Thrown if the pool tries to grow while a max count is set and the max has
        /// already been reached.</exception>
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

        /// <summary>
        /// Returns a <see cref="T"/> instance to the pool.
        /// </summary>
        /// <param name="instance">An instance of <see cref="T"/> to return to the pool.</param>
        /// <exception cref="Exception">Thrown if the instance given is null.</exception>
        /// <exception cref="Exception">Thrown if the instance given is already in the pool.</exception>
        public void Release(T instance) => AddInstance(instance);

        private T CreateInstance()
        {
            m_InstanceCount++;
            return m_InstanceCreator.Invoke();
        }

        private void AddInstance(T instance) {
            Debug.Assert(instance != null, "Cannot add a null instance to the pool!");
            Debug.Assert(m_InstanceSet.Add(instance), "Instance already exists in pool!");

            // Instance count may increase via Populate() or releasing instances not created by the pool
            m_InstanceCount = Math.Max(m_InstanceCount, m_InstanceSet.Count);
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
