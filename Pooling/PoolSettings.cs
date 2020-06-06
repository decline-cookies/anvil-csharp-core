using System;

namespace Anvil.CSharp.Pooling
{
    public class PoolSettings
    {
        public static readonly PoolSettings DEFAULT = new PoolSettings();

        public int InitialCount { get; }
        public int? MaxCount { get; }

        public IGrowthOperator GrowthOperator { get; }

        public PoolSettings(
            int initialCount = 0,
            int? maxCount = null,
            IGrowthOperator growthOperator = null)
        {
            InitialCount = Math.Max(0, initialCount);
            MaxCount = maxCount;
            GrowthOperator = growthOperator ?? new MultiplicativeGrowth(2);

            if (MaxCount.HasValue)
            {
                MaxCount = Math.Max(1, MaxCount.Value);
                InitialCount = Math.Min(MaxCount.Value, InitialCount);
            }
        }
    }
}
