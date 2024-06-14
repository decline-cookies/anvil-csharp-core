namespace Anvil.CSharp.Mathematics
{
    /// <summary>
    /// Useful math utilities
    /// </summary>
    public static class MathUtil
    {
        public const float FLOATING_POINT_EQUALITY_TOLERANCE = 0.00001f;

        /// <summary>
        /// Calculates the Nth prime number that is passed in
        /// </summary>
        /// <param name="nthPrimeNumberToFind">The Nth prime number to find.
        /// Ex.
        /// 1 = the first prime number or 2,
        /// 100 = the 100th prime number or 541
        /// </param>
        /// <returns>The Nth prime number</returns>
        public static long FindPrimeNumber(int nthPrimeNumberToFind)
        {
            int count = 0;
            long a = 2;
            while (count < nthPrimeNumberToFind)
            {
                long b = 2;
                int prime = 1;
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }

                    b++;
                }

                if (prime > 0)
                {
                    count++;
                }

                a++;
            }

            return (--a);
        }

        /// <summary>
        /// Calculate the closest delta between two integers wrapping through the value's limits if required.
        /// </summary>
        /// <example>
        /// ClosestDelta(int.MaxValue - 1, int.MinValue + 1) == 3;
        /// </example>
        /// /// <example>
        /// ClosestDelta(int.MinValue + 1, int.MaxValue - 1) == -3;
        /// </example>
        public static int ClosestDelta(int val1, int val2)
        {
            uint uVal2 = unchecked((uint)val2);
            uint uVal1 = unchecked((uint)val1);
            uint uDelta = uVal2 - uVal1;

            return unchecked((int)uDelta);
        }
    }
}