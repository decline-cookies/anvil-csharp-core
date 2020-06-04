using System;

namespace Anvil.CSharp.Pooling
{
    public class PoolSettings
    {
        public PoolSettings(
            int initialCount = 0,
            ItemStoreType itemStoreType = ItemStoreType.Queue,
            IGrowthOperator growthOperator = null,
            int? maxCount = null)
        {
            InitialCount = Math.Max(0, initialCount);
            ItemStoreType = itemStoreType;
            GrowthOperator = growthOperator ?? new MultiplicativeGrowth(2);
            MaxCount = maxCount;

            if (MaxCount.HasValue)
            {
                MaxCount = Math.Max(1, MaxCount.Value);
                InitialCount = Math.Min(MaxCount.Value, InitialCount);
            }
        }

        public int InitialCount { get; }
        public ItemStoreType ItemStoreType { get; }

        public IGrowthOperator GrowthOperator { get; }

        public int? MaxCount { get; }
    }
}
