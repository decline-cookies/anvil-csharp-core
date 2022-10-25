using System;
using Anvil.CSharp.Mathematics;
using NUnit.Framework;

namespace Anvil.CSharp.Tests
{
    public static class MathUtilTests
    {
        [Test]
        public static void FindPrimeNumber()
        {
            //TODO: #129 - There's probably a more NUnit way to do this
            int[] primeNumbers = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            for (int i = 0; i < primeNumbers.Length; i++)
            {
                Assert.That(MathUtil.FindPrimeNumber(i), Is.EqualTo(primeNumbers[i]));
            }

            Assert.That(MathUtil.FindPrimeNumber(100), Is.EqualTo(541));
        }
    }
}