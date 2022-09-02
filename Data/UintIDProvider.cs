using System;

namespace Anvil.CSharp.Data
{
    
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="uint"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class UintIDProvider : AbstractIDProvider<uint>
    {
        /// <summary>
        /// Triggers the first time that any instance of <see cref="UintIDProvider"/> passes its
        /// <see cref="AbstractIDProvider{T}.SupplyWarningThreshold"/> for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event EventHandler<IDLimitWarningEventArgs> OnIDLimitGlobalWarning
        {
            add => Internal_OnIDLimitGlobalWarning += value;
            remove => Internal_OnIDLimitGlobalWarning -= value;
        }
        
        /// <inheritdoc cref="AbstractIDProvider{T}"/>
        public UintIDProvider(uint supplyWarningThreshold = uint.MaxValue - 1_000_000) : base(supplyWarningThreshold)
        {
        }

        protected override uint IncrementID(uint currentID)
        {
            return ++currentID;
        }

        protected override bool HasIDExceededSupplyWarningThreshold(uint currentID)
        {
            return currentID > SupplyWarningThreshold;
        }
    }
}