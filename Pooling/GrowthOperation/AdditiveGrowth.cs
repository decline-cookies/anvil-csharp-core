using System;
using Anvil.CSharp.Logging;

namespace Anvil.CSharp.Pooling
{
    /// <summary>
    /// Increases a pool's size additively.
    /// </summary>
    public class AdditiveGrowth : IGrowthOperator
    {
        /// <summary>
        /// Constructs an <see cref="AdditiveGrowth"/> instance from the given <paramref name="step"/> size.
        /// </summary>
        /// <param name="step">The step size when increasing additively.</param>
        public AdditiveGrowth(int step)
        {
            if (step <= 0)
            {
                Log.GetLogger(this).Error($"Step must be greater than zero: {step}. Setting to 1");
                step = 1;
            }
            GrowthStep = step;
        }

        /// <summary>
        /// The step size when increasing additively.
        /// </summary>
        public int GrowthStep { get; }

        /// <summary>
        /// Calculates by how much a pool should grow given the current size.
        /// </summary>
        /// <param name="currentCount">The current size of the pool.</param>
        /// <returns>The amount the pool should grow by.</returns>
        public int CalculateGrowthStep(int currentCount) => GrowthStep;
    }
}
