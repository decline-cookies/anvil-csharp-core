using System;
using System.Diagnostics;
using Anvil.CSharp.Logging;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// An instance that provides a unique ID each time it is requested.
    /// Includes a mechanism to detect when ID supply is near exhaustion.
    /// </summary>
    public class IDProvider
    {
        /// <summary>
        /// Arguments that describe an ID limit warning.
        /// </summary>
        public class IDLimitWarningEventArgs : EventArgs
        {
            /// <summary>
            /// Indicates whether the limit warning has been handled. If true, no further corrective action is required
            /// by the application.
            /// </summary>
            public bool IsHandled { get; private set; }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            internal IDLimitWarningEventArgs()
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
        /// Triggers the first time that any instance of <see cref="IDProvider"/> passes its
        /// <see cref="SupplyWarningThreshold"/> for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event EventHandler<IDLimitWarningEventArgs> OnIDLimitGlobalWarning;

        /// <summary>
        /// A reserved ID to represent "No ID".
        /// <see cref="GetNextID"/> will never return this value even when IDs roll over and are no longer unique.
        /// </summary>
        public const uint UNSET_ID = 0;

        /// <summary>
        /// Triggers the first time that <see cref="SupplyWarningThreshold"/> is passed.
        /// This gives the consumer the opportunity to react before IDs are exhausted.
        /// </summary>
        public event EventHandler<IDLimitWarningEventArgs> OnIDLimitWarning;

        /// <summary>
        /// The threshold which, when passed, triggers <seealso cref="OnIDLimitWarning"/>.
        /// </summary>
        public readonly uint SupplyWarningThreshold;

        private bool m_HasIDWarningTriggered = false;
        private uint m_LastNewID = 0;


        /// <summary>
        /// Creates a new ID provider optionally allowing a supply warning threshold to be set.
        /// </summary>
        /// <param name="supplyWarningThreshold">
        /// (default: <see cref="uint.MaxValue"/>) The threshold which, when passed, triggers
        /// <see cref="OnIDLimitImminent"/>.
        /// </param>
        public IDProvider(uint supplyWarningThreshold = uint.MaxValue - 1_000_000)
        {
            SupplyWarningThreshold = supplyWarningThreshold;
        }

        /// <summary>
        /// Provide an unused ID.
        /// </summary>
        /// <returns>A unique ID.</returns>
        /// <remarks>
        /// If <see cref="uint"/> has rolled over an error will be logged and IDs are no longer guaranteed to be unique.
        /// </remarks>
        public uint GetNextID()
        {
            uint id = ++m_LastNewID;
            CheckIfIDThresholdCrossed();

            if (m_LastNewID == 0)
            {
                Log.GetLogger(this).Error($"{nameof(IDProvider)} has passed its maximum ID value. IDs provided are no longer guaranteed to be unique.");
                // Push the ID past the UNSET_ID value so at least THAT remains unique.
                id = ++m_LastNewID;
            }

            Debug.Assert(id != UNSET_ID);
            return id;
        }

        private void CheckIfIDThresholdCrossed()
        {
            if (m_LastNewID <= SupplyWarningThreshold || m_HasIDWarningTriggered)
            {
                return;
            }

            Log.GetLogger(this).Warning($"{nameof(IDProvider)} has passed its supply warning threshold. Threshold: {SupplyWarningThreshold:N0}");
            m_HasIDWarningTriggered = true;

            IDLimitWarningEventArgs args = new IDLimitWarningEventArgs();
            OnIDLimitWarning?.Invoke(this, args);
            OnIDLimitGlobalWarning?.Invoke(this, args);
        }
    }
}