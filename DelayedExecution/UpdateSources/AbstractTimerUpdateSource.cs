using Anvil.CSharp.Logging;
using System;
using System.Threading;

namespace Anvil.CSharp.DelayedExecution
{
    //TODO: Explore code gen to avoid having the developer subclass. (Related: AbstractOffThreadUpdateSource)
    //Subclass and create a constructorless implementation that locks in the interval and thread marshalling
    
    /// <summary>
    /// Provides a time constrained update source.
    /// This <see cref="Timer"/> based implementation trades off accuracy for performance.
    /// Very short intervals (~15ms) may not work as expected.
    /// See: https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer?view=net-5.0#remarks
    /// If short, accurate intervals are required <see cref="AbstractThreadSleepUpdateSource"/> is the more appropriate choice.
    /// </summary>
    public abstract class AbstractTimerUpdateSource : AbstractUpdateSource
    {
        private Timer m_Timer;

        protected AbstractTimerUpdateSource(float updateInterval)
        {
            InitTimer(updateInterval, (state) => DispatchOnUpdateEvent());
        }

        protected AbstractTimerUpdateSource(float updateInterval, Action<Action> onThreadMarshall)
        {
            InitTimer(updateInterval, (state) => onThreadMarshall(DispatchOnUpdateEvent));
        }

        private void InitTimer(float updateInterval, TimerCallback callback)
        {
            int updateIntervalMs = (int)SecondsToMilliseconds(updateInterval);
            WarnIfInvalidInterval(updateIntervalMs);

            m_Timer = new Timer(
                callback,
                null,
                updateIntervalMs,
                updateIntervalMs);
        }

        protected override void DisposeSelf()
        {
            m_Timer?.Dispose();

            base.DisposeSelf();
        }

        private void WarnIfInvalidInterval(int interval)
        {
            // Update interval less than 15ms will not work as expected.
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer?view=net-5.0#remarks
            if (interval < 15)
            {
                Logger.Warning($"Intervals less than 15ms will not work reliably. Consider using ({nameof(AbstractThreadSleepUpdateSource)})\n See https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer?view=net-5.0#remarks");
            }
        }

        private static float SecondsToMilliseconds(float seconds) => MathF.Round(seconds * 1000);
    }
}
