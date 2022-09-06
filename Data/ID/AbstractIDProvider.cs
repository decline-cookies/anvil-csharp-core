using Anvil.CSharp.Core;
using Anvil.CSharp.Logging;
using System;
using System.Diagnostics;

namespace Anvil.CSharp.Data
{
    /// <inheritdoc cref="AbstractIDProvider"/>
    /// <typeparam name="T">The underlying type of the id</typeparam>
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
        /// The threshold which, when passed, triggers <seealso cref="AbstractIDProvider.OnIDLimitWarning"/>.
        /// </summary>
        public readonly T SupplyWarningThreshold;
        
        private T m_LastNewID = default;
        private bool m_HasIDWarningTriggered = false;
        
        /// <summary>
        /// Creates a new ID provider optionally allowing a supply warning threshold to be set.
        /// </summary>
        /// <param name="supplyWarningThreshold">
        /// The threshold which, when passed, triggers
        /// <see cref="AbstractIDProvider.OnIDLimitWarning"/>.
        /// </param>
        protected AbstractIDProvider(T supplyWarningThreshold) : base()
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
                Log.GetLogger(this).Error($"{GetType().Name} has passed its maximum ID value. IDs provided are no longer guaranteed to be unique.");
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

            Log.GetLogger(this).Warning($"{GetType().Name} has passed its supply warning threshold. Threshold: {SupplyWarningThreshold:N0}");
            m_HasIDWarningTriggered = true;
    
            ID.IDLimitWarningEventArgs args = new ID.IDLimitWarningEventArgs();
            DispatchIDLimitWarning(args);
            ID.DispatchOnIDLimitGlobalWarning(this, args);
        }
    }

    /// <summary>
    /// An instance that provides a unique ID each time it is requested.
    /// Includes a mechanism to detect when ID supply is near exhaustion.
    /// </summary>
    public class AbstractIDProvider : AbstractAnvilBase
    {
        /// <summary>
        /// Triggers the first time that <see cref="AbstractIDProvider{T}.SupplyWarningThreshold"/> is passed.
        /// This gives the consumer the opportunity to react before IDs are exhausted.
        /// </summary>
        public event EventHandler<ID.IDLimitWarningEventArgs> OnIDLimitWarning;

        protected override void DisposeSelf()
        {
            OnIDLimitWarning = null;
            base.DisposeSelf();
        }

        protected void DispatchIDLimitWarning(ID.IDLimitWarningEventArgs args)
        {
            OnIDLimitWarning?.Invoke(this, args);
        }
    }
}
