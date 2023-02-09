using System;
using System.Collections.Generic;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Facilitates accumulating and deferring work to be executed at a specific, periodic, moment dictated by an
    /// <see cref="AbstractUpdateSource"/>
    /// </summary>
    /// <typeparam name="TUpdateSource">The update source to schedule work against.</typeparam>
    public class DeferredWorkPump<TUpdateSource> : DeferredDataPump<TUpdateSource, Action> where TUpdateSource : AbstractUpdateSource
    {
        /// <summary>
        /// Creates an instance of the work pump.
        /// </summary>
        /// <param name="willEagerExecuteWork">
        /// (optional) If true, the work scheduled while executing accumulated work will get executed during the same
        /// update.Otherwise, the work is deferred to the next update.
        /// </param>
        public DeferredWorkPump(bool willEagerExecuteWork = false) : base(ExecutePendingWork, willEagerExecuteWork) { }

        /// <inheritdoc cref="DeferredDataPump{TUpdateSource,TDataType}.Schedule"/>
        public new void Schedule(Action work)
        {
            base.Schedule(work);
        }

        private static void ExecutePendingWork(IEnumerator<Action> pendingWork)
        {
            while (pendingWork.MoveNext())
            {
                pendingWork.Current();
            }
        }
    }
}