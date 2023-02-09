using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Facilitates accumulating and deferring work to be executed at a specific, periodic, moment dictated by an
    /// <see cref="AbstractUpdateSource"/>
    /// </summary>
    /// <typeparam name="T">The update source to schedule work against.</typeparam>
    public class DeferredWorkPump<T> : AbstractAnvilBase where T : AbstractUpdateSource
    {
        private readonly Action m_ExecutePendingWorkStrategy;
        private readonly Queue<Action> m_PendingWork;
        private readonly UpdateHandle m_Update;
        private CallAfterHandle m_PendingWorkRequestHandle;

        /// <summary>
        /// Returns true if the accumulated work is currently executing.
        /// </summary>
        public bool IsExecutingWork
        {
            get => m_Update.IsUpdating;
        }

        /// <summary>
        /// Creates an instance of the work pump.
        /// </summary>
        /// <param name="willEagerExecuteWork">
        /// (optional) If true, the work scheduled while executing accumulated work will get executed during the same
        /// update.Otherwise, the work is deferred to the next update.
        /// </param>
        public DeferredWorkPump(bool willEagerExecuteWork = false)
        {
            m_ExecutePendingWorkStrategy = willEagerExecuteWork ? (Action)ExecutePendingWork_Eager : ExecutePendingWork;

            m_Update = UpdateHandle.Create<T>();
            m_PendingWork = new Queue<Action>();
        }

        protected override void DisposeSelf()
        {
            m_Update.Dispose();

            base.DisposeSelf();
        }

        /// <summary>
        /// Schedules work to be executed during the next update phase.
        /// </summary>
        /// <param name="work"></param>
        public void ScheduleWork(Action work)
        {
            m_PendingWork.Enqueue(work);
            m_PendingWorkRequestHandle ??= m_Update.CallAfterUpdates(0, m_ExecutePendingWorkStrategy);
        }

        private void ExecutePendingWork()
        {
            // Cache the count before iterating so any new work scheduled during the current import work is pushed
            // off to the next update.
            m_PendingWorkRequestHandle = null;
            int count = m_PendingWork.Count;

            for (int i = 0; i < count; i++)
            {
                m_PendingWork.Dequeue()();
            }
        }

        private void ExecutePendingWork_Eager()
        {
            while (m_PendingWork.Count > 0)
            {
                m_PendingWork.Dequeue()();
            }

            m_PendingWorkRequestHandle = null;
        }
    }
}
