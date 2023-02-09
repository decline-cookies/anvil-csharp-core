using Anvil.CSharp.Logging;
using System;

namespace Anvil.CSharp.Data
{
    public static class ID
    {
        /// <summary>
        /// Arguments that describe an ID limit warning.
        /// </summary>
        public class LimitWarningEventArgs : EventArgs
        {
            /// <summary>
            /// Indicates whether the limit warning has been handled. If true, no further corrective action is required
            /// by the application.
            /// </summary>
            public bool IsHandled { get; private set; }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            internal LimitWarningEventArgs()
            {
                IsHandled = false;
            }

            /// <summary>
            /// Call if the limit has been handled and no corrective action is required by the application.
            /// The event will continue to be raised to handlers for informational purposes (Logging, analytics, etc..)
            /// </summary>
            /// <remarks>
            /// Calling <see cref="Handle"/> multiple times on a single event will raise a warning. While handling the
            /// limit multiple times probably isn't harmful, it's not recommended.
            /// </remarks>
            public void Handle()
            {
                if (IsHandled)
                {
                    Log.GetLogger(this).Warning($"ID supply limit has already been handled by another listener and shouldn't get handled again.");
                }

                IsHandled = true;
            }
        }

        /// <summary>
        /// Triggers the first time that any instance of <see cref="AbstractIDProvider"/> passes its
        /// SupplyWarningThreshold for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event EventHandler<LimitWarningEventArgs> OnIDLimitGlobalWarning;

        /// <summary>
        /// Resets the static state.
        /// </summary>
        /// <remarks>
        /// Used for runtimes where a domain reload doesn't occur between run sessions (Ex: Unity).
        /// </remarks>
        public static void ResetState()
        {
            OnIDLimitGlobalWarning = null;
        }

        internal static void DispatchOnIDLimitGlobalWarning(object sender, LimitWarningEventArgs args)
        {
            OnIDLimitGlobalWarning?.Invoke(sender, args);
        }
    }
}
