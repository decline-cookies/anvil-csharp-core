namespace Anvil.CSharp.Pooling
{
    /// <summary>
    /// Performs a pool growth operation.
    /// </summary>
    public interface IGrowthOperator
    {
        /// <summary>
        /// Calculates by how much a pool should grown given the current size.
        /// </summary>
        /// <param name="currentCount">The current size of the pool.</param>
        /// <returns>The amount the pool should grow by.</returns>
        int CalculateGrowthStep(int currentCount);
    }
}
