using Anvil.CSharp.DelayedExecution;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Anvil.CSharp.DelayedExecution
{
    //TODO: Keep? See AbstractTimerUpdateSource
    public class AbstractOffThreadUpdateSource : AbstractUpdateSource
    {
        private readonly CancellationTokenSource m_CancelationTokenSource;
        private readonly int m_UpdateIntervalMs;
        private readonly Action m_OnIntervalAction;

        public AbstractOffThreadUpdateSource(float updateInterval)
        {
            m_UpdateIntervalMs = (int)SecondsToMs(updateInterval);
            m_OnIntervalAction = DispatchOnUpdateEvent;

            m_CancelationTokenSource = new CancellationTokenSource();
            Start();
        }

        public AbstractOffThreadUpdateSource(float updateInterval, Action<Action> onThreadMarshall)
        {
            m_UpdateIntervalMs = (int)SecondsToMs(updateInterval);
            m_OnIntervalAction = () => onThreadMarshall(DispatchOnUpdateEvent);
            
            m_CancelationTokenSource = new CancellationTokenSource();
            Start();
        }

        protected override void DisposeSelf()
        {
            Stop();
            m_CancelationTokenSource.Dispose();

            base.DisposeSelf();
        }

        private void Start()
        {
            Task result = Task.Run(
                () => RunUpdateLoop(m_UpdateIntervalMs, m_OnIntervalAction), 
                m_CancelationTokenSource.Token);
        }

        private void Stop()
        {
            m_CancelationTokenSource.Cancel();
        }

        private static float SecondsToMs(float seconds) => MathF.Round(seconds * 1000);

        private static async Task RunUpdateLoop(int updateInterval, Action onInterval)
        {
            while (true)
            {
                await Task.Delay(updateInterval);
                onInterval();
            }
        }


    }
}
