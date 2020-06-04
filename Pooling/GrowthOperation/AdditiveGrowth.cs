namespace Anvil.CSharp.Pooling
{
    public class AdditiveGrowth : IGrowthOperator
    {
        public AdditiveGrowth(int step) => GrowthStep = step;

        public int GrowthStep { get; }

        public int CalculateGrowthStep(int currentCount) => GrowthStep;
    }
}
