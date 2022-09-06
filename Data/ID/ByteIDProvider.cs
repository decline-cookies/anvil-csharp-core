namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="byte"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class ByteIDProvider : AbstractIDProvider<byte>
    {
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
