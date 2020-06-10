using System;

namespace Anvil.CSharp.Pooling
{
    public class AdditiveGrowth : IGrowthOperator
    {
        public AdditiveGrowth(int step)
        {
            if (step <= 0)
            {
                Console.Error.WriteLine($"Step must be greater than zero: {step}. Setting to 1");
                step = 1;
            }
            GrowthStep = step;
        }

        public int GrowthStep { get; }

        public int CalculateGrowthStep(int currentCount) => GrowthStep;
    }
}
