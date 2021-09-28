using System;
using Anvil.CSharp.Logging;

namespace Anvil.CSharp.Pooling
{
    /// <summary>
    /// Increases a pool's size multiplicatively.
    /// </summary>
    public class MultiplicativeGrowth : IGrowthOperator
    {
        /// <summary>
        /// Constructs a <see cref="MultiplicativeGrowth"/> instance from the given <paramref name="multiplier"/>.
        /// </summary>
        /// <param name="multiplier">The value to multiply the previous pool size by when growing.</param>
        public MultiplicativeGrowth(float multiplier)
        {
            if (multiplier < 1)
            {
                Log.GetLogger(this).Error($"Multiplier must not be less than one: {multiplier}. Setting to 1.");
                multiplier = 1;
            }

            GrowthMultiplier = multiplier;
        }

        /// <summary>
        /// The value to multiple the previous pool size by when growing.
        /// </summary>
        public float GrowthMultiplier { get; }

        /// <summary>
        /// Calculates by how much a pool should grow given the current size, rounded to the nearest integer.
        /// </summary>
        /// <param name="currentCount">The current size of the pool.</param>
        /// <returns>The amount the pool should grow by.</returns>
        public int CalculateGrowthStep(int currentCount)
        {
            return (int)Math.Round(currentCount * GrowthMultiplier) - currentCount;
        }
    }
}
