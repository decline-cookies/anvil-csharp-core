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
        /// Triggers the first time that any instance of <see cref="IDProvider"/> passes its
        /// <see cref="SupplyWarningThreshold"/> for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event Action<IDProvider> OnIDLimitGlobalWarning;

        /// <summary>
        /// A reserved ID to represent "No ID".
        /// <see cref="GetNextID"/> will never return this value even when IDs roll over and are no longer unique.
        /// </summary>
        public const int UNSET_ID = 0;

        /// <summary>
        /// Triggers the first time that <see cref="SupplyWarningThreshold"/> is passed.
        /// This gives the consumer the opportunity to react before IDs are exhausted.
        /// </summary>
        public event Action OnIDLimitWarning;

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
        public IDProvider(uint supplyWarningThreshold = uint.MaxValue-1)
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
            if (m_LastNewID > SupplyWarningThreshold && m_HasIDWarningTriggered)
            {
                Log.GetLogger(this).Warning($"{nameof(IDProvider)} has passed its supply warning threshold. Threshold: {SupplyWarningThreshold}");
                m_HasIDWarningTriggered = true;
                OnIDLimitWarning?.Invoke();
                OnIDLimitGlobalWarning?.Invoke(this);
            }
        }
    }
}