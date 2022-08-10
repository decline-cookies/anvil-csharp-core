using System;
using System.Collections;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Iteration
{
    /// <summary>
    /// Runs a method up to the next yield return.
    /// If the method returns an <see cref="IEnumerable"/> or <see cref="IEnumerator"/> it will
    /// be immediately run until it encounters a yield, another <see cref="IEnumerable"/>/<see cref="IEnumerator"/>
    /// or completion.
    /// Otherwise, this class does not evaluate what is returned by the method.
    /// </summary>
    /// <remarks>
    /// - The immediate recursive execution behaviour of nested methods is a contrast to Unity's
    ///   <see cref="UnityEngine.Coroutine"/> where every yield defers to the next call
    /// - This class is useful for amortizing a method's execution over multiple calls (ex: Frames)
    /// </remarks>
    public class YieldedMethodRunner : AbstractAnvilBase
    {
        /// <summary>
        /// Reflects whether calling <see cref="RunToYield"/> will result in any further progression.
        /// This value is true when either <see cref="Terminate"/> has been called or the method has completed.
        /// </summary>
        public bool IsComplete { get; private set; }

        private readonly Stack<IEnumerator> m_MethodContextStack;
        private IEnumerator m_CurrentMethodContext;

        /// <summary>
        /// Creates a new <see cref="YieldedMethodRunner"/> instance.
        /// Execution does not start until <see cref="RunToYield"/> is called.
        /// </summary>
        /// <param name="method">The yielded method to run</param>
        public YieldedMethodRunner(Func<IEnumerator> method):this(method()) { }

        private YieldedMethodRunner(IEnumerator methodContext)
        {
            // Pre-allocate a capacity of 3 since most yield stacks will likely never go deeper.
            m_MethodContextStack = new Stack<IEnumerator>(3);
            m_CurrentMethodContext = methodContext;
        }

        /// <summary>
        /// Run the <see cref="IEnumerator"/> to the next yield (or method completion).
        /// </summary>
        public void RunToYield()
        {
            bool keepRunning = true;
            while(!IsComplete && keepRunning)
            {
                // Keep going until a non-scope changing yield is hit or we're complete
                keepRunning = false;
                bool isCurrentContextComplete = !m_CurrentMethodContext.MoveNext();

                if (!isCurrentContextComplete)
                {
                    if (TryEnterInnerContext())
                    {
                        keepRunning = true;
                    }
                }
                else if (m_MethodContextStack.Count > 0)
                {
                    ExitCurrentContext();
                    keepRunning = true;
                }
                else
                {
                    IsComplete = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Terminates the runner and prevents any further progression past the next `yield`.
        /// Any subsequent calls to <see cref="RunToYield"/> will return immediately.
        /// </summary>
        public void Terminate()
        {
            IsComplete = true;
        }

        private bool TryEnterInnerContext()
        {
            IEnumerator innerContext = GetInnerContext();
            if (innerContext != null)
            {
                m_MethodContextStack.Push(m_CurrentMethodContext);
                m_CurrentMethodContext = innerContext;
                return true;
            }

            return false;
        }

        private void ExitCurrentContext()
        {
            m_CurrentMethodContext = m_MethodContextStack.Pop();
        }

        private IEnumerator GetInnerContext()
        {
            object lastYieldedReturn = m_CurrentMethodContext.Current;

#if DEBUG
            // Catch accidental use of IEnumerable instead of IEnumerator.
            // IEnumerable is less efficient because we need to get an IEnumerator.
            if (lastYieldedReturn is IEnumerable innerEnumerable)
            {
                LogIEnumerableUse();
                return innerEnumerable.GetEnumerator();
            }
#endif

            return lastYieldedReturn as IEnumerator;
        }

        private void LogIEnumerableUse()
        {
            Logger.Warning($"{nameof(YieldedMethodRunner)} was provided {nameof(IEnumerable)} should be provided {nameof(IEnumerator)}. (inefficient)");
        }
    }
}