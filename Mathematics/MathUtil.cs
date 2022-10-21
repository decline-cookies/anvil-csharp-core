namespace Anvil.CSharp.Mathematics
{
    /// <summary>
    /// Useful helpers to aid in debugging
    /// </summary>
    public static class MathUtil
    {
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
        /// Returns the number of digits in an integer.
        /// Optionally, includes the negative sign in the count.
        /// </summary>
        /// <param name="value">The value to evaluate</param>
        /// <param name="countSign">(default:false) If true, will increase the count by one for all numbers less than 0</param>
        /// <returns>The number of digits in an integer</returns>
        /// <remarks>Taken from: https://stackoverflow.com/a/51099524/640196</remarks>
        public static int GetDigitCount(int value, bool countSign = false)
        {
            if (value >= 0)
            {
                if (value < 10) return 1;
                if (value < 100) return 2;
                if (value < 1000) return 3;
                if (value < 10000) return 4;
                if (value < 100000) return 5;
                if (value < 1000000) return 6;
                if (value < 10000000) return 7;
                if (value < 100000000) return 8;
                if (value < 1000000000) return 9;
                return 10;
            }
            else
            {
                int signCount = countSign ? 1 : 0;
                if (value > -10) return signCount + 2;
                if (value > -100) return signCount + 3;
                if (value > -1000) return signCount + 4;
                if (value > -10000) return signCount + 5;
                if (value > -100000) return signCount + 6;
                if (value > -1000000) return signCount + 7;
                if (value > -10000000) return signCount + 8;
                if (value > -100000000) return signCount + 9;
                if (value > -1000000000) return signCount + 10;
                return signCount + 11;
            }
        }
    }
}