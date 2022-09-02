using Anvil.CSharp.Logging;
using System.Diagnostics;
using System;

namespace Anvil.CSharp.Data
{
    
    /// <inheritdoc cref="AbstractIDProvider"/>
    /// <typeparam name="T">The underlying type for the id</typeparam>
    public abstract class AbstractIDProvider<T> : AbstractIDProvider
        //Constraints get as close as possible to only numeric types
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable 
    {
        /// <summary>
        /// A reserved ID to represent "No ID".
        /// <see cref="GetNextID"/> will never return this value even when IDs roll over and are no longer unique.
        /// </summary>
        public static readonly T UNSET_ID = default;
        
        /// <summary>
        /// Triggers the first time that <see cref="SupplyWarningThreshold"/> is passed.
        /// This gives the consumer the opportunity to react before IDs are exhausted.
        /// </summary>
        public event EventHandler<IDLimitWarningEventArgs> OnIDLimitWarning;
        
        /// <summary>
        /// The threshold which, when passed, triggers <seealso cref="OnIDLimitWarning"/>.
        /// </summary>
        public readonly T SupplyWarningThreshold;
        
        private T m_LastNewID = default;
        private bool m_HasIDWarningTriggered = false;
        
        /// <summary>
        /// Creates a new ID provider optionally allowing a supply warning threshold to be set.
        /// </summary>
        /// <param name="supplyWarningThreshold">
        /// The threshold which, when passed, triggers
        /// <see cref="OnIDLimitWarning"/>.
        /// </param>
        protected AbstractIDProvider(T supplyWarningThreshold)
        {
            SupplyWarningThreshold = supplyWarningThreshold;
        }
        
        /// <summary>
        /// Provide an unused ID.
        /// </summary>
        /// <returns>A unique ID.</returns>
        /// <remarks>
        /// If <typeparamref name="T"/> has rolled over an error will be logged and IDs are no longer guaranteed to be unique.
        /// </remarks>
        public T GetNextID()
        {
            m_LastNewID = IncrementID(m_LastNewID);
            CheckIfIDThresholdCrossed();

            if (m_LastNewID.Equals(default))
            {
                Log.GetLogger(this).Error($"{nameof(AbstractIDProvider<T>)} has passed its maximum ID value. IDs provided are no longer guaranteed to be unique.");
                // Push the ID past the UNSET_ID value so at least THAT remains unique.
                m_LastNewID = IncrementID(m_LastNewID);
            }

            Debug.Assert(!m_LastNewID.Equals(UNSET_ID));
            return m_LastNewID;
        }

        protected abstract T IncrementID(T currentID);
        protected abstract bool HasIDExceededSupplyWarningThreshold(T currentID);
        
        private void CheckIfIDThresholdCrossed()
        {
            if (!HasIDExceededSupplyWarningThreshold(m_LastNewID)|| m_HasIDWarningTriggered)
            {
                return;
            }

            Log.GetLogger(this).Warning($"{nameof(AbstractIDProvider<T>)} has passed its supply warning threshold. Threshold: {SupplyWarningThreshold:N0}");
            m_HasIDWarningTriggered = true;

            IDLimitWarningEventArgs args = new AbstractIDProvider.IDLimitWarningEventArgs();
            OnIDLimitWarning?.Invoke(this, args);
            DispatchOnIDLimitGlobalWarning(args);
        }
    }
    
    /// <summary>
    /// An instance that provides a unique ID each time it is requested.
    /// Includes a mechanism to detect when ID supply is near exhaustion.
    /// </summary>
    public abstract class AbstractIDProvider
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
        /// Triggers the first time that any instance of <see cref="AbstractIDProvider"/> passes its
        /// SupplyWarningThreshold for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        internal static event EventHandler<IDLimitWarningEventArgs> Internal_OnIDLimitGlobalWarning;

        protected void DispatchOnIDLimitGlobalWarning(IDLimitWarningEventArgs args)
        {
            Internal_OnIDLimitGlobalWarning?.Invoke(this, args);
        }
    }
}
