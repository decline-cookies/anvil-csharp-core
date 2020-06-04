namespace Anvil.CSharp.Pooling
{
    public interface IGrowthOperator
    {
        int CalculateGrowthStep(int currentCount);
    }
}
