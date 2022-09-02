using System;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="byte"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class ByteIDProvider : AbstractIDProvider<byte>
    {
        /// <summary>
        /// Triggers the first time that any instance of <see cref="ByteIDProvider"/> passes its
        /// <see cref="AbstractIDProvider{T}.SupplyWarningThreshold"/> for the first time.
        /// This gives the application the opportunity to react before IDs are exhausted.
        /// </summary>
        public static event EventHandler<IDLimitWarningEventArgs> OnIDLimitGlobalWarning
        {
            add => Internal_OnIDLimitGlobalWarning += value;
            remove => Internal_OnIDLimitGlobalWarning -= value;
        }
        
        /// <inheritdoc cref="AbstractIDProvider{T}"/>
        public ByteIDProvider(byte supplyWarningThreshold = byte.MaxValue - 32) : base(supplyWarningThreshold)
        {
        }

        protected override byte IncrementID(byte currentID)
        {
            return ++currentID;
        }

        protected override bool HasIDExceededSupplyWarningThreshold(byte currentID)
        {
            return currentID > SupplyWarningThreshold;
        }
    }
}
