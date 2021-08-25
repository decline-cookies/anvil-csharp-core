using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Provides a time constrained update source.
    /// This <see cref="Thread.Sleep(int)"/> based implementation trades off performance for accuracy.
    /// It is best suited to very short intervals. If the interval is larger than ~15ms then 
    /// <see cref="AbstractTimerUpdateSource"/> may be a more appropriate choice.
    /// </summary>
    public class AbstractThreadSleepUpdateSource : AbstractUpdateSource
    {
        private int m_UpdateIntervalMS;
        private Action m_OnIntervalAction;
        private CancellationTokenSource m_CancellationTokenSource;

        public AbstractThreadSleepUpdateSource(float updateInterval)
        {
            Init(updateInterval, DispatchOnUpdateEvent);
        }

        public AbstractThreadSleepUpdateSource(float updateInterval, Action<Action> onThreadMarshall)
        {
            Init(updateInterval, () => onThreadMarshall(DispatchOnUpdateEvent));
        }

        private void Init(float updateInterval, Action onIntervalAction)
        {
            m_UpdateIntervalMS = (int)SecondsToMilliseconds(updateInterval);
            m_OnIntervalAction = onIntervalAction;

            m_CancellationTokenSource = new CancellationTokenSource();
            Start();
        }

        protected override void DisposeSelf()
        {
            Stop();
            m_CancellationTokenSource.Dispose();

            base.DisposeSelf();
        }

        private void Start()
        {
            Task result = Task.Run(
                () => RunUpdateLoop(m_UpdateIntervalMS, m_OnIntervalAction, m_CancellationTokenSource.Token), 
                m_CancellationTokenSource.Token);
        }

        private void Stop()
        {
            m_CancellationTokenSource.Cancel();
        }

        private static float SecondsToMilliseconds(float seconds) => MathF.Round(seconds * 1000);

        private static void RunUpdateLoop(int updateInterval, Action onInterval, CancellationToken cancelToken)
        {
            Thread.Sleep(updateInterval);

            while (!cancelToken.IsCancellationRequested)
            {
                onInterval();
                Thread.Sleep(updateInterval);
            }
        }


    }
}
