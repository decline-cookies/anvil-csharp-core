using System;
using System.Threading;

namespace Anvil.CSharp.DelayedExecution
{
    //TODO: Document
    //TODO: Explore code gen to avoid having the developer subclass. (Related: AbstractOffThreadUpdateSource)
    //Subclass and create a constructorless implementation that locks in the interval and thread marshalling
    public abstract class AbstractTimerUpdateSource : AbstractUpdateSource
    {
        private readonly Timer m_Timer;

        protected AbstractTimerUpdateSource(float updateInterval)
        {
            int updateIntervalMs = (int)SecondsToMs(updateInterval);

            m_Timer = new Timer(
                (state) => DispatchOnUpdateEvent(), 
                null, 
                updateIntervalMs, 
                updateIntervalMs);
        }

        protected AbstractTimerUpdateSource(float updateInterval, Action<Action> onThreadMarshall)
        {
            int updateIntervalMs = (int)SecondsToMs(updateInterval);

            m_Timer = new Timer(
                (state) => onThreadMarshall(DispatchOnUpdateEvent), 
                null, 
                updateIntervalMs, 
                updateIntervalMs);
        }

        protected override void DisposeSelf()
        {
            m_Timer.Dispose();

            base.DisposeSelf();
        }

        private static float SecondsToMs(float seconds) => MathF.Round(seconds * 1000);
    }
}
