using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Facilitates accumulating and deferring data to be processed at a specific, periodic, moment dictated by an
    /// <see cref="AbstractUpdateSource"/>.
    /// </summary>
    /// <typeparam name="TUpdateSource">The update source to schedule data against.</typeparam>
    /// <typeparam name="TDataType">The type of data to accumulate.</typeparam>
    public class DeferredDataPump<TUpdateSource, TDataType> : AbstractAnvilBase where TUpdateSource : AbstractUpdateSource
    {
        private readonly Action<IEnumerator<TDataType>> m_ProcessPendingData;
        private readonly bool m_WillEagerExecuteDeferredData;
        private readonly UpdateHandle m_Update;
        private readonly Queue<TDataType> m_PendingData;

        private CallAfterHandle m_PendingDataScheduleHandle;


        /// <summary>
        /// Returns true if the pending data is currently being processed.
        /// </summary>
        public bool IsExecuting
        {
            get => m_Update.IsUpdating;
        }

        /// <summary>
        /// Creates an instance of the data pump.
        /// </summary>
        /// <param name="processPendingData">A callback that will process the data that was deferred.</param>
        /// <param name="willEagerExecuteWork">
        /// (optional) If true, the data scheduled while processing accumulated data will get processed during the same
        /// update. Otherwise, the data is deferred to the next update.
        /// </param>
        public DeferredDataPump(Action<IEnumerator<TDataType>> processPendingData, bool willEagerExecuteDeferredData = false)
        {
            m_ProcessPendingData = processPendingData;
            m_WillEagerExecuteDeferredData = willEagerExecuteDeferredData;

            m_Update = UpdateHandle.Create<TUpdateSource>();
            m_PendingData = new Queue<TDataType>();
        }

        protected override void DisposeSelf()
        {
            m_Update.Dispose();

            base.DisposeSelf();
        }

        /// <summary>
        /// Schedules data to to be processed during the next update phase.
        /// </summary>
        /// <param name="data">The data to process.</param>
        public void Schedule(TDataType data)
        {
            m_PendingData.Enqueue(data);
            m_PendingDataScheduleHandle ??= ScheduleProcessPendingData();
        }

        private CallAfterHandle ScheduleProcessPendingData()
        {
            return m_Update.CallAfterUpdates(0, ProcessPendingData);
        }

        private void ProcessPendingData()
        {
            // (IEnumerator<TDataType>) cast is required for <C# 9.0 compatibility. (CS8957)
            using IEnumerator<TDataType> enumerator = m_WillEagerExecuteDeferredData
                ? (IEnumerator<TDataType>) new EagerDestructiveQueueEnumerator(m_PendingData)
                : (IEnumerator<TDataType>) new SnapshotDestructiveQueueEnumerator(m_PendingData);

            m_ProcessPendingData(enumerator);

            m_PendingDataScheduleHandle = !m_WillEagerExecuteDeferredData && m_PendingData.Count > 0
                ? ScheduleProcessPendingData()
                : null;
        }

        // ----- Inner Types ----- //
        /// <summary>
        /// An enumerator that will dequeue elements in a <see cref="Queue{T}"/> as iterated.
        /// The enumerator allows the underlying queue to be modified while iterating but will only deliver the number
        /// of elements that existed when the enumerator was created.
        /// </summary>
        /// <remarks>
        /// It's assumed that any modifications to the <see cref="Queue{T}"/> are additive. dequeue the elements that
        /// exist when the enumerator was created. Reducing the number of elements elements during iteration will throw
        /// an exception.
        /// </remarks>
        private struct SnapshotDestructiveQueueEnumerator : IEnumerator<TDataType>
        {
            private readonly Queue<TDataType> m_Queue;
            private TDataType m_Current;
            private int m_ElementsRemaining;
            private bool m_IsValid;

            public TDataType Current
            {
                get
                {
                    if (!m_IsValid)
                    {
                        // Either it's passed the end of the collection, disposed, or MoveNext hasn't been called at least once
                        throw new InvalidOperationException("Enumerator is not in a valid state.");
                    }

                    return m_Current;
                }
            }

            object IEnumerator.Current
            {
                get => Current;
            }

            public SnapshotDestructiveQueueEnumerator(Queue<TDataType> queue)
            {
                m_Queue = queue;
                m_ElementsRemaining = m_Queue.Count;
                m_IsValid = false;
                m_Current = default;
            }

            public void Dispose()
            {
                m_ElementsRemaining = -2;
                m_Current = default;
            }

            public bool MoveNext()
            {
                if (m_ElementsRemaining > 0)
                {
                    m_IsValid = true;
                    m_Current = m_Queue.Dequeue();
                    --m_ElementsRemaining;
                    Debug.Assert(m_ElementsRemaining <= m_Queue.Count);
                }
                else
                {
                    m_IsValid = false;
                }

                return m_IsValid;
            }

            public void Reset()
            {
                throw new NotSupportedException("Enumeration is a destructive action. Enumerator cannot be reset.");
            }
        }

        /// <summary>
        /// An enumerator that will dequeue elements in a <see cref="Queue{T}"/> as iterated.
        /// The enumerator allows the underlying queue to be modified while iterating and will deliver all elements in
        /// the collection. Even elements that are added after the enumerator was created.
        /// </summary>
        private struct EagerDestructiveQueueEnumerator : IEnumerator<TDataType>
        {
            private readonly Queue<TDataType> m_Queue;
            private TDataType m_Current;
            private bool m_IsValid;

            public TDataType Current
            {
                get
                {
                    if (!m_IsValid)
                    {
                        // Either it's passed the end of the collection, disposed, or MoveNext hasn't been called at least once
                        throw new InvalidOperationException("Enumerator is not in a valid state.");
                    }

                    return m_Current;
                }
            }

            object IEnumerator.Current
            {
                get => Current;
            }

            public EagerDestructiveQueueEnumerator(Queue<TDataType> queue)
            {
                m_Queue = queue;
                m_IsValid = false;
                m_Current = default;
            }

            public void Dispose()
            {
                m_IsValid = false;
                m_Current = default;
            }

            public bool MoveNext()
            {
                m_IsValid = m_Queue.TryDequeue(out m_Current);
                return m_IsValid;
            }

            public void Reset()
            {
                throw new NotSupportedException("Enumeration is a destructive action. Enumerator cannot be reset.");
            }
        }
    }
}