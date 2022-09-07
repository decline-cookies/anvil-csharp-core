namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Specific implementation of <see cref="AbstractIDProvider{T}"/> for <see cref="uint"/>
    /// </summary>
    /// <inheritdoc cref="AbstractIDProvider{T}"/>
    public class IDProvider : AbstractIDProvider<uint>
    {
        /// <inheritdoc cref="AbstractIDProvider{T}"/>
        public IDProvider(uint supplyWarningThreshold = uint.MaxValue - 1_000_000) : base(supplyWarningThreshold)
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
