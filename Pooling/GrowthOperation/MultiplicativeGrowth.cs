using System;

namespace Anvil.CSharp.Pooling
{
    public class MultiplicativeGrowth : IGrowthOperator
    {
        public MultiplicativeGrowth(float multiplier)
        {
            if (multiplier < 1)
            {
                Console.Error.WriteLine($"Multiplier must not be less than one: {multiplier}. Setting to 1.");
                multiplier = 1;
            }

            GrowthMultiplier = multiplier;
        }

        public float GrowthMultiplier { get; }

        public int CalculateGrowthStep(int currentCount)
        {
            return (int)Math.Round(currentCount * GrowthMultiplier) - currentCount;
        }
    }
}
