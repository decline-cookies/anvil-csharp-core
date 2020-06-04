using System;

namespace Anvil.CSharp.Pooling
{
    public class MultiplicativeGrowth : IGrowthOperator
    {
        public MultiplicativeGrowth(float multiplier) => GrowthMultiplier = multiplier;

        public float GrowthMultiplier { get; }

        public int CalculateGrowthStep(int currentCount)
        {
            return (int)Math.Round(currentCount * GrowthMultiplier) - currentCount;
        }
    }
}
